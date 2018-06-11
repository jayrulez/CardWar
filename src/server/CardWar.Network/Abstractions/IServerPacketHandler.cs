using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Network.Abstractions
{
    public interface IServerPacketHandler
    {
        string PacketType { get; }
        Task Handle(IConnection connection, byte[] packetBytes);
    }
}
