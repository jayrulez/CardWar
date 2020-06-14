﻿using System.Collections.Generic;

namespace CardWar.Network.Abstractions
{
    public interface IPacketConverter
    {
        byte[] ToBytes<T>(T packet) where T : Packet;
        T FromBytes<T>(byte[] packetBytes) where T : Packet;
        IAsyncEnumerable<T> StreamFromBytes<T>(byte[] packetBytes) where T : Packet;
        T Unwrap<T>(Packet packet) where T : Packet;
    }
}
