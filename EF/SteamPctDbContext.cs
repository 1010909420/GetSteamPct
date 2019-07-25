using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Model.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace EF
{
    public class SteamPctDbContext : DbContext
    {
        public DbSet<Goods> goods { get; set; }
        public DbSet<Activity> activity { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("sql.json");
            var configuration = builder.Build();

            string connStr = configuration["ConnectionString"];
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(connStr);
            }
        }
    }
}
