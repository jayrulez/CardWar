using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Server.Data
{
    public class ServerDbContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<XmlKey> XmlKeys { get; set; }

        public ServerDbContext(DbContextOptions<ServerDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");

            builder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasAlternateKey(e => e.Username);
            });

            builder.Entity<XmlKey>(entity =>
            {
                entity.HasKey(e => e.Name);
            });
        }
    }
}
