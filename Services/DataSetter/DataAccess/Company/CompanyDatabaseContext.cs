using DataSetter.DataAccess.Company.Entities;

using Microsoft.EntityFrameworkCore;

namespace DataSetter.DataAccess.Company;

public sealed class CompanyDatabaseContext : DbContext
{
    public DbSet<Entities.Company> Companies { get; set; } = null!;
    public DbSet<Industry> Industries { get; set; } = null!;
    public DbSet<Sector> Sectors { get; set; } = null!;

    public CompanyDatabaseContext(DbContextOptions<CompanyDatabaseContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
        modelBuilder.Entity<Entities.Company>().HasIndex(x => x.Name).IsUnique();
        base.OnModelCreating(modelBuilder);
    }
}