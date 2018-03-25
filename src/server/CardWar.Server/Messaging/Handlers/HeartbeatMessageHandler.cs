using CardWar.Common.Messaging;
using CardWar.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace CardWar.Server.Messaging.Handlers
{
    public class HeartBeatMessageHandler : IMessageHandler
    {
        private readonly ILogger _logger;

        public HeartBeatMessageHandler(
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HeartBeatMessageHandler>();
        }

        public object HandleMessage(object message)
        {
            var heartHeatMessage = message as HeartBeatMessageData;

            if(heartHeatMessage != null)
            {
                _logger.LogInformation($"HeartBeat: {DateTime.Now.Ticks}");

                return message;
            }

            return null;
        }
    }
}
