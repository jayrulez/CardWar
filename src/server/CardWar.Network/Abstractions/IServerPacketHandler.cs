using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Network.Abstractions
{
    public interface IServerPacketHandler
    {
        Task Handle(IConnection connection, byte[] packetBytes);
    }

    public interface IServerPacketHandler<T> : IServerPacketHandler where T : Packet
    {
    }
}
