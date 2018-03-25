using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CardWar.Messages;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;
using CardWar.Server.Services;
using CardWar.Server.Messaging.Handlers;
using CardWar.Common.Sockets;
using CardWar.Common.Messaging;
using Newtonsoft.Json.Linq;

namespace CardWar.Server
{
    public class ServerSocketHandler : SocketHandler
    {
        private readonly ConcurrentDictionary<string, object> _sessions;
        private readonly TimerService _timerService;
        private readonly LoginMessageHandler _loginMessageHandler;
        private readonly HeartBeatMessageHandler _heartBeatMessageHandler;

        public ServerSocketHandler(SocketConnectionManager socketConnectionManager,
            TimerService timerService,
            ILoggerFactory loggerFactory,
            LoginMessageHandler loginMessageHandler,
            HeartBeatMessageHandler heartBeatMessageHandler)
            : base(socketConnectionManager, loggerFactory.CreateLogger<ServerSocketHandler>())
        {
            _loginMessageHandler = loginMessageHandler;
            _heartBeatMessageHandler = heartBeatMessageHandler;

            _sessions = new ConcurrentDictionary<string, object>();
            _timerService = timerService;
        }

        public override async Task ReceiveAsync(Socket socket, string message)
        {
            var socketId = SocketConnectionManager.GetId(socket);

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var messageToken = JToken.Parse(message);

            var type = (int)messageToken["Type"];
            var data = messageToken["Data"];

            if (type == (int)MessageType.Invalid)
            {
                var responseMessage = new Message
                {
                    Type = type,
                    Data = data
                };

                await SendMessageAsync(socket, JsonConvert.SerializeObject(responseMessage));

                return;
            }

            switch (type)
            {
                case (int)MessageType.Login:
                    var loginMessage = JsonConvert.DeserializeObject<LoginMessageData>(data.ToString());
                    var loginResult = _loginMessageHandler.HandleMessage(loginMessage);

                    await SendMessageAsync(socket, JsonConvert.SerializeObject(new Message
                    {
                        Type = (int)MessageType.LoginSuccessful,
                        Data = loginResult
                    }));

                    break;

                case (int)MessageType.Heartbeat:
                    var heartBeatMessage = JsonConvert.DeserializeObject<HeartBeatMessageData>(data.ToString());

                    var heartBeat = _heartBeatMessageHandler.HandleMessage(heartBeatMessage);

                    await SendMessageAsync(socket, JsonConvert.SerializeObject(heartBeat));

                    break;
            }

            // if message is not from authenticated user
            // try to parse message as LoginMessage
            // if message is LoginMessage
            // attempt to authenticate credentials
            // if credentials are authenticated
            // add session for user

            // if message is from authenticated user, parse and process message using relevant message handler

            // How is a session represented?
            // A session should have a unique ID, the user info as well as a socket.
            // A session can be updated to change the socket in cases where socket was close and user reconnects by using an auth token
            // The auth token should be the unique ID of the session?
            // Maybe make the session property a list of tuples (Id, User, Socket) ?

            // Only one session will be maintained per user
            // If a user with an existing token logs in again then remove the existing session and close the socket

            // Well, after thinking about it, let us handle sessions differently
            // A session should represent a user's interaction with the server.
            // We will need state management to track what is happening in a session.
        }
    }
}