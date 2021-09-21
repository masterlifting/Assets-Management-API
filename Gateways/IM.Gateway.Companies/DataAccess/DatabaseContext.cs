using IM.Gateway.Companies.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Gateway.Companies.DataAccess
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<StockSplit> StockSplits { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StockSplit>().HasKey(x => new { x.CompanyTicker, x.Date });
        }
    }
}
