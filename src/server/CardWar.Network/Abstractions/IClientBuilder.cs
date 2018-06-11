using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Network.Abstractions
{
    public interface IClientBuilder
    {
        IClient Build<T>() where T : IClient;
        IClientBuilder RegisterPacketHandler<TPacketType, TPacketHandlerType>();
    }
}
