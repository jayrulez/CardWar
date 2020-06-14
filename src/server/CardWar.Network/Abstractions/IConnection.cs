﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace CardWar.Network.Abstractions
{
    public interface IConnection
    {
        string Id { get; }
        bool IsClosed { get; }
        Task Send<T>(T packet) where T : Packet;
        IAsyncEnumerable<Packet> GetPackets();
        void Close();
    }
}
