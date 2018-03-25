using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CardWar.Common.Sockets
{
    public class SocketConnectionManager
    {
        private readonly ILogger _logger;

        private ConcurrentDictionary<string, Socket> _sockets = new ConcurrentDictionary<string, Socket>();

        public SocketConnectionManager(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SocketConnectionManager>();
        }

        private string CreateConnectionId()
        {
            return Guid.NewGuid().ToString();
        }

        public Socket GetSocketById(string id)
        {
            return _sockets.FirstOrDefault(p => p.Key == id).Value;
        }

        public ConcurrentDictionary<string, Socket> GetAll()
        {
            return _sockets;
        }

        public string GetId(Socket socket)
        {
            return _sockets.FirstOrDefault(p => p.Value == socket).Key;
        }
        public void AddSocket(Socket socket)
        {
            _sockets.TryAdd(CreateConnectionId(), socket);
        }

        public Task RemoveSocket(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _logger.LogInformation($"Attempting to remove socket '{id}'.");

                _sockets.TryRemove(id, out Socket socket);

                _logger.LogInformation($"Socket '{id}' removed from sockets collection.");

                if (socket != null && socket.Connected)
                {
                    _logger.LogInformation($"Attempting to close socket '{id}'.");

                    socket.Close();

                    _logger.LogInformation($"Closed socket '{id}'.");
                }
            }

            return Task.CompletedTask;
        }
    }
}
