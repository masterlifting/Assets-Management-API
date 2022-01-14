using DataSetter.DataAccess.CompanyData.Entities;

using Microsoft.EntityFrameworkCore;

namespace DataSetter.DataAccess.CompanyData;

public sealed class CompanyDataDatabaseContext : DbContext
{
    public DbSet<Entities.Company> Companies { get; set; } = null!;
    public DbSet<SourceType> SourceTypes { get; set; } = null!;
    public DbSet<CompanySourceType> CompanySourceTypes { get; set; } = null!;

    public DbSet<Price> Prices { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<StockSplit> StockSplits { get; set; } = null!;
    public DbSet<StockVolume> StockVolumes { get; set; } = null!;

    public CompanyDataDatabaseContext(DbContextOptions<CompanyDataDatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CompanySourceType>()
            .HasKey(x => new { x.CompanyId, x.SourceTypeId });
        modelBuilder.Entity<CompanySourceType>()
            .HasOne(x => x.Company)
            .WithMany(x => x.CompanySourceTypes)
            .HasForeignKey(x => x.CompanyId);
        modelBuilder.Entity<CompanySourceType>()
            .HasOne(x => x.SourceType)
            .WithMany(x => x.CompanySourceTypes)
            .HasForeignKey(x => x.SourceTypeId);

        modelBuilder.Entity<Price>().HasKey(x => new { x.CompanyId, x.Date });
        modelBuilder.Entity<StockSplit>().HasKey(x => new { x.CompanyId, x.Date });
        modelBuilder.Entity<StockVolume>().HasKey(x => new { x.CompanyId, x.Date });
        modelBuilder.Entity<Report>().HasKey(x => new { x.CompanyId, x.Year, x.Quarter });

        modelBuilder.Entity<Price>().HasOne(x => x.Company).WithMany(x => x.Prices).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StockSplit>().HasOne(x => x.Company).WithMany(x => x.StockSplits).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<StockVolume>().HasOne(x => x.Company).WithMany(x => x.StockVolumes).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Report>().HasOne(x => x.Company).WithMany(x => x.Reports).OnDelete(DeleteBehavior.Cascade);
    }
}