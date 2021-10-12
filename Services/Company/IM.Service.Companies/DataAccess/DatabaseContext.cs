using IM.Service.Companies.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Companies.DataAccess
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Industry> Industries { get; set; } = null!;
        public DbSet<Sector> Sectors { get; set; } = null!;
        public DbSet<StockSplit> StockSplits { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseSerialColumns();
            modelBuilder.Entity<StockSplit>().HasKey(x => new { x.CompanyTicker, x.Date });
        }
    }
}
