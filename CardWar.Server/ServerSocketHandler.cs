using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CardWar.Messages;
using CardWar.Server.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Server
{
    public class ServerSocketHandler : SocketHandler
    {
        private readonly ConcurrentDictionary<string, object> _sessions;

        public ServerSocketHandler(SocketConnectionManager socketConnectionManager, ILoggerFactory loggerFactory) : base(socketConnectionManager, loggerFactory.CreateLogger<ServerSocketHandler>())
        {
            _sessions = new ConcurrentDictionary<string, object>();
        }

        public override async Task ReceiveAsync(Socket socket, string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                return;
            }

            var messageObject = JsonConvert.DeserializeObject<Message>(message);

            if(messageObject == null)
            {
                var responseMessage = new Message
                {

                };

                await SendMessageAsync(socket, JsonConvert.SerializeObject(responseMessage));

                return;
            }

            var socketId = SocketConnectionManager.GetId(socket);
            
            if (!_sessions.ContainsKey(socketId))
            {
                Logger.LogInformation($"received from unauthenticated client: {message}");

                _sessions.AddOrUpdate(socketId, socket, (oldKey, oldValue) => socket);
            }
            else
            {
                Logger.LogInformation($"received from authenticated client: {message}");
            }
            
            await SendMessageAsync(socket, message);
        }
    }
}