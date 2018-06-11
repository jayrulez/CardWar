using CardWar.Network.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace CardWar.Server
{
    class GameServer : TcpServer
    {
        public GameServer(IServiceProvider provider) : base(provider)
        {
            _logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger<GameServer>();
        }
    }
}
