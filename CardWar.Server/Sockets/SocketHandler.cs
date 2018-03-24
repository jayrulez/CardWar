using Microsoft.Extensions.Logging;
using CardWar.Server.Managers;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Server.Sockets
{
    public abstract class SocketHandler
    {
        protected SocketConnectionManager SocketConnectionManager { get; set; }
        protected ILogger Logger { get; set; }

        public SocketHandler(SocketConnectionManager socketConnectionManager, ILogger logger)
        {
            SocketConnectionManager = socketConnectionManager;

            Logger = logger;
        }

        public virtual Task OnConnected(Socket socket)
        {
            SocketConnectionManager.AddSocket(socket);

            return Task.CompletedTask;
        }

        public virtual async Task OnDisconnected(Socket socket)
        {
            await SocketConnectionManager.RemoveSocket(SocketConnectionManager.GetId(socket));
        }

        public async Task SendMessageAsync(Socket socket, string message)
        {
            if (!socket.Connected)
                return;
            
            await socket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(message), offset: 0, count: message.Length), socketFlags: SocketFlags.None);
        }

        public async Task SendMessageAsync(string socketId, string message)
        {
            await SendMessageAsync(SocketConnectionManager.GetSocketById(socketId), message);
        }

        public async Task SendMessageToAllAsync(string message)
        {
            foreach (var pair in SocketConnectionManager.GetAll())
            {
                if (pair.Value.Connected)
                    await SendMessageAsync(pair.Value, message);
            }
        }

        public virtual Task ReceiveAsync(Socket socket, string message)
        {
            Logger.LogInformation($"Received: {message}");

            return Task.CompletedTask;
        }
    }
}
