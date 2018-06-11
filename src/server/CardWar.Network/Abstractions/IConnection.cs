using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CardWar.Network.Abstractions
{
    public interface IConnection
    {
        bool Closed { get; }
        string Id { get; }
        Task Send<T>(T packet) where T : Packet;
        Task <(Packet Packet, byte[] PacketBytes)> GetPacket();
        void Close();
    }
}
