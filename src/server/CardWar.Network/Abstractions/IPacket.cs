using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Network.Abstractions
{
    public interface IPacket
    {
        string PacketId { get; set; }

        string Key { get; }
    }
}
