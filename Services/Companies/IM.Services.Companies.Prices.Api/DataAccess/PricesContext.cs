using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Services.Companies.Prices.Api.DataAccess
{
    public class PricesContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Price> Prices { get; set; }
        public DbSet<PriceSourceType> PriceSourceTypes { get; set; }

        public PricesContext(DbContextOptions<PricesContext> options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Price>().HasKey(x => new { x.TickerName, x.Date });
            modelBuilder.Entity<Price>().HasOne(x => x.Ticker).WithMany(x => x.Prices).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PriceSourceType>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<PriceSourceType>().HasData(new PriceSourceType[]
            {
                new (){Id = 1, Name = "moex" },
                new (){Id = 2, Name = "tdameritrade" }
            });
        }
    }
}