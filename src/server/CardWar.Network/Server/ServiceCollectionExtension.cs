using CardWar.Network.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CardWar.Network.Common;

namespace CardWar.Network.Server
{
    public static class ServiceCollectionExtension
    {
        public static ServerBuilder<T> AddTcpServer<T>(this IServiceCollection serviceCollection, IConfiguration configuration) where T : class, IServer
        {
            serviceCollection.AddOptions();
            serviceCollection.Configure<ServerConfiguration>(configuration);

            serviceCollection.AddTransient<TimerService, TimerService>();
            serviceCollection.AddTransient<IPacketSerializer, JsonPacketSerializer>();
            serviceCollection.AddSingleton<ConnectionManager, ConnectionManager>();

            serviceCollection.AddHostedService<T>();

            return new ServerBuilder<T>(serviceCollection);
        }
    }
}
