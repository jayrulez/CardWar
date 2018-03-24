using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CardWar.Server.Data
{
    public class ServerDbContextFactory : IDesignTimeDbContextFactory<ServerDbContext>
    {
        public ServerDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ServerDbContext>();
            
            builder.UseNpgsql("User ID=postgres;Password=postgres;Server=localhost;Port=5432;Database=CardWar;Pooling=true;");

            return new ServerDbContext(builder.Options);
        }
    }
}
