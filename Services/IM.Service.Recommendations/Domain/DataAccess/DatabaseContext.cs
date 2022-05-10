using IM.Service.Recommendations.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Recommendations.Domain.DataAccess;

public class DatabaseContext : DbContext
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Purchase> Purchases { get; set; } = null!;
    public DbSet<Sale> Sales { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }
}