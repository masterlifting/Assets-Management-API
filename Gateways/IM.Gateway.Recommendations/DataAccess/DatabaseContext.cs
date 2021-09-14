using IM.Gateway.Recommendations.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Gateway.Recommendations.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; } = null!;
        public DbSet<Purchase> Purchases { get; set; } = null!;
        public DbSet<Sale> Sales { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
    }
}