using CardWar.Network.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CardWar.Network.Server
{
    public class ServerBuilder<T> : IServerBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        public ServerBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public ServerBuilder<T> RegisterPacketHandler<TPacketHandlerType>()
        {
            _serviceCollection.AddTransient(typeof(IServerPacketHandler), typeof(TPacketHandlerType));

            return this;
        }
    }
}
