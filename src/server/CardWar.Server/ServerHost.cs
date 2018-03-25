using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CardWar.Server.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CardWar.Server.Services;

namespace CardWar.Server
{
    public class ServerHost : IHostedService
    {
        private readonly ILogger _logger;
        private readonly ServerConfig _config;
        private TcpListener _listener;
        private readonly ServerSocketHandler _serverSocketHandler;
        private readonly TimerService _timerService;

        private ConcurrentBag<Task> _tasks;
        private CancellationTokenSource _tasksCancellationTokenSource;
        private CancellationToken _tasksCancellationToken;
        private Task _listenerTask;

        public ServerHost(IOptions<ServerConfig> config, TimerService timerService, ServerSocketHandler serverSocketHandler, ILoggerFactory loggerFactory)
        {
            _config = config?.Value;

            if (_config == null)
            {
                throw new Exception($"Server config is invalid.");
            }

            _serverSocketHandler = serverSocketHandler;
            _timerService = timerService;

            _tasks = new ConcurrentBag<Task>();
            _tasksCancellationTokenSource = new CancellationTokenSource();
            _tasksCancellationToken = _tasksCancellationTokenSource.Token;

            _logger = loggerFactory.CreateLogger<ServerHost>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting service");

            var ipAddress = IPAddress.Loopback;

            _listener = new TcpListener(ipAddress, _config.Port);

            _listener.Start();

            _logger.LogInformation($"Listening on {ipAddress}:{_config.Port}");

            _timerService.Start(_tasksCancellationToken);
            //var timerTask = Task.Factory.StartNew(() => _timerService.Start(_tasksCancellationToken), _tasksCancellationToken);

            //_tasks.Add(timerTask);

            _listenerTask = Task.Factory.StartNew(() => ListenLoop(_tasksCancellationToken), _tasksCancellationToken);

            _tasks.Add(_listenerTask);

            return Task.CompletedTask;
        }

        private async void ListenLoop(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The ListenLoop task was cancelled.");

                cancellationToken.ThrowIfCancellationRequested();
            }

            for (; ; )
            {
                var socket = await _listener.AcceptSocketAsync();

                if (socket == null)
                {
                    break;
                }

                var handlerTask = Task.Factory.StartNew(() => HandleSocket(socket, cancellationToken), cancellationToken);



                _tasks.Add(handlerTask);
            }
        }

        private async Task HandleSocket(Socket socket, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                socket.Close();

                cancellationToken.ThrowIfCancellationRequested();
            }

            await _serverSocketHandler.OnConnected(socket);

            while (socket.Connected)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("The HandleSocket task was cancelled.");

                    cancellationToken.ThrowIfCancellationRequested();
                }

                var networkStream = new NetworkStream(socket, true);
                var memoryStream = new MemoryStream();
                var streamReader = new StreamReader(memoryStream);
                byte[] buffer = new byte[4096];

                await networkStream.ReadAsync(buffer, 0, buffer.Length);

                var message = Encoding.UTF8.GetString(buffer).Trim('\0');

                await _serverSocketHandler.ReceiveAsync(socket, message);
            }

            await _serverSocketHandler.OnDisconnected(socket);

            socket.Close();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping service");

            _tasksCancellationTokenSource.Cancel();

            try
            {
                Task.WaitAll(_tasks.ToArray());
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

            foreach (var task in _tasks)
            {
                _logger.LogInformation($"Task '{task.Id}' is now '{task.Status}'.");
            }

            //_listenerTask.Wait();
            //_listenerTask.Dispose();


            //_listener.Server.Disconnect(false);
            //_listener.Server.Shutdown(SocketShutdown.Both);
            //_listener.Server.Close();

            return Task.CompletedTask;
        }
    }
}
