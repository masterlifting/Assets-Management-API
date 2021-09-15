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
    }
}
