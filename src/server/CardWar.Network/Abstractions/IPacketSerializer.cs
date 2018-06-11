using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Network.Abstractions
{
    public interface IPacketSerializer
    {
        byte[] Serialize(Packet packet);
        T Deserialize<T>(byte[] serializedPacket) where T : Packet;
        (T Packet, byte[] PacketBytes) Deserialize<T>(string serializedPacket) where T : Packet;
    }
}
