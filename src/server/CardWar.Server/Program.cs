using CardWar.Network.Server;
using CardWar.Server.PacketHandlers;
using CardWar.Packets;
using CardWar.Server.Data;
using CardWar.Server.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;
using CardWar.Network.Abstractions;
using CardWar.Network.Common;

namespace CardWar.Server
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContextPool<ApplicationDbContext>(options =>
                    {
                        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(migrationsAssembly));
                    });

                    services.AddDataProtection()
                        .AddKeyManagementOptions(options =>
                        {
                            options.XmlRepository = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<IXmlRepository>();
                        });


                    services.AddTransient<TimerService, TimerService>();
                    services.AddTransient<IPacketSerializer, JsonPacketSerializer>();
                    services.AddSingleton<ConnectionManager, ConnectionManager>();


                    services.AddOptions();
                    services.Configure<ServerConfiguration>(hostContext.Configuration.GetSection("ServerConfiguration"));

                    services.AddTransient<IServerPacketHandler<PingRequestPacket>, PingRequestPacketHandler>();
                    
                    services.AddSingleton<IXmlRepository, DatabaseXmlRepository>();
                    services.AddSingleton<UserManager, UserManager>();
                    services.AddSingleton<SessionManager, SessionManager>();

                    /*
                    services.AddTcpServer<GameServer>(hostContext.Configuration.GetSection("ServerConfiguration"))
                        .RegisterPacketHandler<PingRequestPacketHandler>();
                        */
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            var host = builder.UseConsoleLifetime().Build();

            var server = new GameServer2(host);

            server.AddPacketHandler<PingRequestPacket, PingRequestPacketHandler>();

            await server.StartAsync();

            host.Services.GetRequiredService<IApplicationLifetime>().ApplicationStopping.Register(server.StopAsync);
            
            await host.RunAsync();
        }
    }
}
