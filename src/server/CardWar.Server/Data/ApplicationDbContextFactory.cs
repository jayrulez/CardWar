using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CardWar.Server.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            builder.UseNpgsql("User ID=postgres;Password=postgres;Server=localhost;Port=5432;Database=CardWar;Pooling=true;");

            return new ApplicationDbContext(builder.Options);
        }
    }
}