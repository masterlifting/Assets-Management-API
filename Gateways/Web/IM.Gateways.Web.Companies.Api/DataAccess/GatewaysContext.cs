using IM.Gateways.Web.Companies.Api.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Gateways.Web.Companies.Api.DataAccess
{
    public class GatewaysContext : DbContext
    {
        public DbSet<Company> Companies { get; set; } = null!;

        public GatewaysContext(DbContextOptions<GatewaysContext> options) : base(options)
        {
            //delete
            Database.EnsureCreated(); 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
