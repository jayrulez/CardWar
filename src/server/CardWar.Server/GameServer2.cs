using CardWar.Network.Server;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Server
{
    public class GameServer2 : AbstractTcpServer
    {
        public GameServer2(IHost host) : base(host)
        {
        }
    }
}
