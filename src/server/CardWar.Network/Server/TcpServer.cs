using CardWar.Network.Abstractions;
using CardWar.Network.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CardWar.Network.Server
{
    public abstract class TcpServer : IServer
    {
        protected readonly ServerConfiguration _configuration;
        protected readonly TimerService _timerService;
        protected readonly ConnectionManager _connectionManager;
        protected readonly IPacketSerializer _packetSerializer;

        protected readonly CancellationTokenSource _tasksCancellationTokenSource;
        protected readonly CancellationToken _tasksCancellationToken;

        protected readonly ConcurrentBag<Task> _serverTasks;
        protected readonly Dictionary<string, IServerPacketHandler> _packetHandlers;

        protected ILogger _logger;
        protected TcpListener _listener;
        protected Task _listenerTask;

        public TcpServer(IServiceProvider provider)
        {
            _tasksCancellationTokenSource = new CancellationTokenSource();
            _tasksCancellationToken = _tasksCancellationTokenSource.Token;

            _configuration = provider.GetRequiredService<IOptions<ServerConfiguration>>().Value;

            _timerService = provider.GetRequiredService<TimerService>();

            _connectionManager = provider.GetRequiredService<ConnectionManager>();

            _packetSerializer = provider.GetRequiredService<IPacketSerializer>();

            _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<TcpServer>();

            _serverTasks = new ConcurrentBag<Task>();

            _packetHandlers = new Dictionary<string, IServerPacketHandler>();

            foreach (var packerHandler in provider.GetServices<IServerPacketHandler>())
            {
                _packetHandlers.Add(packerHandler.PacketType, packerHandler);
            }
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service");

            _timerService.Start(_tasksCancellationToken);

            var ipAddress = IPAddress.Loopback;

            _listener = new TcpListener(ipAddress, _configuration.Port);

            _listener.Start();

            _logger.LogInformation($"Listening on {ipAddress}:{_configuration.Port}");

            _listenerTask = Task.Factory.StartNew(() => ListenLoop(_tasksCancellationToken), _tasksCancellationToken);

            _serverTasks.Add(_listenerTask);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
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
                    var conneection = new TcpConnection(socket, _packetSerializer);

                    var handlerTask = Task.Factory.StartNew(() => HandleConnection(conneection, cancellationToken), cancellationToken);

                    _serverTasks.Add(handlerTask);
                }
            }
        }

        protected async Task HandleConnection(IConnection connection, CancellationToken cancellationToken)
        {
            await OnConnected(connection, cancellationToken);

            while (!connection.Closed)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The HandleSocket task was cancelled.");

                    //cancellationToken.ThrowIfCancellationRequested();

                    break;
                }

                var packetData = await connection.GetPacket();

                await OnPacketReceived(connection, packetData.Packet, packetData.PacketBytes);
            }

            await OnDisconnected(connection);
        }

        protected async Task OnPacketReceived(IConnection connection, Packet packet, byte[] packetBytes)
        {
            var packetType = packet.Key;

            if (string.IsNullOrEmpty(packet.Key))
            {
                //throw new Exception($"Invalid packet.");
                return;
            }

            if (_packetHandlers.ContainsKey(packet.Key))
            {
                var handler = _packetHandlers[packet.Key];

                await handler.Handle(connection, packetBytes);
            }
            else
            {
                throw new NotImplementedException($"No Packet Handler has been registered for packet with 'Key'='{packet.Key}'.");
            }
        }
    }
}
