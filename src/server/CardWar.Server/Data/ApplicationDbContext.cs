﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CardWar.Server.Data
{
    public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
    {
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public virtual DbSet<Card> Cards { get; set; }
        public virtual DbSet<Session> Sessions { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");
            
            builder.Entity<Card>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            builder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User).WithMany(e => e.Sessions).HasForeignKey(e => e.UserId);
            });

            builder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasAlternateKey(e => e.Username);
            });
        }
    }
}