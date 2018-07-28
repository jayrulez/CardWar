using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Network.Abstractions
{
    public class Packet : IPacket
    {
        public string PacketId { get; set; }
        public string Key { get => GetType().Name; }
    }
}
