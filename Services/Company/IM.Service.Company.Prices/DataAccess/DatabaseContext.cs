using IM.Service.Company.Prices.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Company.Prices.Enums;

namespace IM.Service.Company.Prices.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<SourceType> SourceTypes { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Price>().HasKey(x => new { x.TickerName, x.Date });
            modelBuilder.Entity<Price>().HasOne(x => x.Ticker).WithMany(x => x.Prices).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SourceType>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<SourceType>().HasData(
                new() { Id = (byte)Enums.PriceSourceTypes.Default, Name = "Select price source!" },
                new() { Id = (byte)Enums.PriceSourceTypes.MOEX, Name = nameof(Enums.PriceSourceTypes.MOEX) },
                new() { Id = (byte)Enums.PriceSourceTypes.Tdameritrade, Name = nameof(Enums.PriceSourceTypes.Tdameritrade) }
            );
        }
    }
}