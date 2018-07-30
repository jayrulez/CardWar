using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Network.Abstractions
{
    public interface IServerPacketHandler
    {
        string PacketId { get; }
        Task Handle(IConnection connection, byte[] packetBytes);
    }

    public interface IServerPacketHandler<T> : IServerPacketHandler where T : Packet
    {
        Type PacketType { get; }
    }
}
