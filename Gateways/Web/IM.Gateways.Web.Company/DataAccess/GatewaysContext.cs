using IM.Gateways.Web.Company.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Gateways.Web.Company.DataAccess
{
    public class GatewaysContext : DbContext
    {
        public DbSet<Entities.Company> Companies { get; set; } = null!;

        public GatewaysContext(DbContextOptions<GatewaysContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
