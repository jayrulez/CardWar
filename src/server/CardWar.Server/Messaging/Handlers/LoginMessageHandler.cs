using CardWar.Common.Messaging;
using CardWar.Messages;
using CardWar.Server.Managers;
using Microsoft.Extensions.Logging;
using System;

namespace CardWar.Server.Messaging.Handlers
{
    public class LoginMessageHandler : IMessageHandler
    {
        private readonly UserManager _userManager;
        private readonly ILogger _logger;

        public LoginMessageHandler(UserManager userManager,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;

            _logger = loggerFactory.CreateLogger<LoginMessageHandler>();
        }

        public object HandleMessage(object message)
        {
            var loginMessage = message as LoginMessageData;

            if(loginMessage != null)
            {
                return new LoginSuccessfulMessageData
                {
                    Id = Guid.NewGuid(),
                    Username = loginMessage.Username,
                    Token = Guid.NewGuid().ToString()
                };
            }

            return null;
        }
    }
}
