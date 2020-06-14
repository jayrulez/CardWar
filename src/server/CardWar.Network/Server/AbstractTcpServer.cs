﻿using CardWar.Common.Utilities;
using CardWar.Network.Abstractions;
using CardWar.Network.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CardWar.Network.Server
{
    public abstract class AbstractTcpServer
    {
        protected readonly ServerOptions _configuration;
        protected readonly TimerService _timerService;
        protected readonly ConnectionManager _connectionManager;
        protected readonly IPacketConverter _packetSerializer;

        protected readonly CancellationTokenSource _tasksCancellationTokenSource;
        protected readonly CancellationToken _tasksCancellationToken;

        protected readonly ConcurrentBag<Task> _serverTasks;
        protected readonly Dictionary<string, IPacketHandler> _packetHandlers;

        protected ILogger _logger;
        protected TcpListener _listener;
        protected Task _listenerTask;
        protected IServiceProvider _serviceProvider;

        private IHost _host;

        public AbstractTcpServer(IHost host)
        {
            _host = host;

            _serviceProvider = _host.Services;

            _tasksCancellationTokenSource = new CancellationTokenSource();
            _tasksCancellationToken = _tasksCancellationTokenSource.Token;

            _configuration = _serviceProvider.GetRequiredService<IOptions<ServerOptions>>().Value;

            _timerService = _serviceProvider.GetRequiredService<TimerService>();

            _connectionManager = _serviceProvider.GetRequiredService<ConnectionManager>();

            _packetSerializer = _serviceProvider.GetRequiredService<IPacketConverter>();

            _logger = _serviceProvider.GetRequiredService<ILogger<AbstractTcpServer>>();

            _serverTasks = new ConcurrentBag<Task>();

            _packetHandlers = new Dictionary<string, IPacketHandler>();
        }

        public virtual Task OnConnected(IConnection connection, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                connection.Close();
            }
            else
            {
                _connectionManager.AddConnection(connection);
            }

            return Task.CompletedTask;
        }

        public virtual Task OnDisconnected(IConnection connection)
        {
            _connectionManager.RemoveConnection(connection);

            return Task.CompletedTask;
        }

        public async Task SendPacket<T>(IConnection connection, T packet) where T : Packet
        {
            await connection.Send(packet);
        }

        public async Task SendPacket<T>(string connectionId, T packet) where T : Packet
        {
            var connection = _connectionManager.GetConnection(connectionId);

            if (connection != null)
            {
                await SendPacket(connection, packet);
            }
        }

        public async Task BroadcastPacket<T>(T packet) where T : Packet
        {
            foreach (var connection in _connectionManager.Connections)
            {
                await SendPacket(connection, packet);
            }
        }

        public AbstractTcpServer AddPacketHandler<TPacket, TPacketHandler>() where TPacket : Packet where TPacketHandler : PacketHandler<TPacket>
        {
            var packetHandler = _serviceProvider.GetService<IPacketHandler<TPacket>>();

            if (packetHandler != null)
            {
                _packetHandlers.Add(typeof(TPacket).FullName, packetHandler);

                _logger.LogInformation($"Packet '{packetHandler.GetType().Name}' registered for packet type '{typeof(TPacket).Name}'.");
            }

            return this;
        }

        protected async void ListenLoop(CancellationToken cancellationToken)
        {
            for (; ; )
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The ListenLoop task was cancelled.");

                    //cancellationToken.ThrowIfCancellationRequested();

                    break;
                }

                var socket = await _listener.AcceptSocketAsync();

                if (socket != null)
                {
                    var connection = new TcpConnection(socket, _packetSerializer);

                    var handlerTask = Task.Factory.StartNew(() => HandleConnection(connection, cancellationToken), cancellationToken);

                    _serverTasks.Add(handlerTask);
                }
            }
        }

        protected async Task HandleConnection(IConnection connection, CancellationToken cancellationToken)
        {
            await OnConnected(connection, cancellationToken);

            while (!connection.IsClosed)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The HandleSocket task was cancelled.");

                    //cancellationToken.ThrowIfCancellationRequested();

                    break;
                }

                await foreach (var packet in connection.GetPackets())
                {
                    await OnPacketReceived(connection, packet);
                }
            }

            await OnDisconnected(connection);
        }

        protected async Task OnPacketReceived(IConnection connection, Packet packet)
        {
            if (string.IsNullOrEmpty(packet.Type))
            {
                //throw new Exception($"Invalid packet.");
                return;
            }

            if (_packetHandlers.ContainsKey(packet.Type))
            {
                var handler = _packetHandlers[packet.Type];
                await handler.Handle(connection, packet);
            }
            else
            {
                throw new NotImplementedException($"No Packet Handler has been registered for packet with 'Key'='{packet.Type}'.");
            }
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting service");

            _timerService.Start(_tasksCancellationToken);

            var ipAddress = IPAddress.Loopback;

            _listener = new TcpListener(ipAddress, _configuration.Port);

            _listener.Start();

            _logger.LogInformation($"Listening on {ipAddress}:{_configuration.Port}");

            _listenerTask = Task.Factory.StartNew(() => ListenLoop(_tasksCancellationToken), _tasksCancellationToken);

            _serverTasks.Add(_listenerTask);

            /*
            var applicationLifetime = _serviceProvider.GetRequiredService<IApplicationLifetime>();

            applicationLifetime.ApplicationStopping.Register(OnShutdown);
            */

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping service");

            _tasksCancellationTokenSource.Cancel();

            try
            {
                Task.WaitAll(_serverTasks.ToArray());
            }
            catch (AggregateException ex)
            {
                _logger.LogInformation("AggregateException thrown with the following inner exceptions:");

                foreach (var exception in ex.InnerExceptions)
                {
                    if (exception is TaskCanceledException)
                    {
                        _logger.LogInformation($"TaskCanceledException: Task {((TaskCanceledException)exception).Task.Id}");
                    }
                    else
                    {
                        _logger.LogInformation($"Exception: {exception.GetType().Name}");
                    }
                }
            }
            finally
            {
                _tasksCancellationTokenSource.Dispose();
            }

            foreach (var task in _serverTasks)
            {
                _logger.LogInformation($"Task '{task.Id}' is now '{task.Status}'.");
            }

            return Task.CompletedTask;
        }

        public void StopAsync()
        {
            StopAsync(default);
        }
    }
}
