using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CardWar.Server.Configuration;
using CardWar.Server.Data;
using CardWar.Server.Managers;
using CardWar.Server.Services;
using CardWar.Server.Sockets;

namespace CardWar.Server
{
    public class Program
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
                    services.AddOptions();
                    services.Configure<ServerConfig>(hostContext.Configuration.GetSection("ServerConfig"));

                    services.AddDbContext<ServerDbContext>(options =>
                    {
                        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly(migrationsAssembly));
                    });

                    services.AddDataProtection()
                        .AddKeyManagementOptions(options =>
                        {
                            options.XmlRepository = services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<IXmlRepository>();
                        });

                    services.AddSingleton<IHostedService, ServerHost>();
                    services.AddSingleton<UserManager, UserManager>();
                    services.AddSingleton<IXmlRepository, DatabaseXmlRepository>();

                    services.AddSingleton<SocketConnectionManager, SocketConnectionManager>();
                    services.AddSingleton<ServerSocketHandler, ServerSocketHandler>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }
    }
}
