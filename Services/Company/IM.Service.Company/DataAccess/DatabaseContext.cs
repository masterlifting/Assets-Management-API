using IM.Service.Company.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.DataAccess
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<Entities.Company> Companies { get; set; } = null!;
        public DbSet<Industry> Industries { get; set; } = null!;
        public DbSet<Sector> Sectors { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseSerialColumns();
        }
    }
}
