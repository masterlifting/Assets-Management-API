using IM.Service.Market.Analyzer.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Analyzer.DataAccess;

public class DatabaseContext : DbContext
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<AnalyzedEntity> AnalyzedEntities { get; set; } = null!;
    public DbSet<AnalyzedEntityType> AnalyzedEntityTypes { get; set; } = null!;
    public DbSet<Rating> Ratings { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Rating>().HasIndex(x => x.Result);
        modelBuilder.Entity<AnalyzedEntity>().HasKey(x => new {x.CompanyId, x.AnalyzedEntityTypeId, x.Date });
        modelBuilder.Entity<AnalyzedEntity>().HasOne(x => x.Company).WithMany(x => x.AnalyzedEntities).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>().HasOne(x => x.Company).WithOne(x => x.Rating).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AnalyzedEntityType>().HasData(
            new()
            {
                Id = (byte)Enums.EntityTypes.Price,
                Name = nameof(Enums.EntityTypes.Price),
            }
            , new()
            {
                Id = (byte)Enums.EntityTypes.Report,
                Name = nameof(Enums.EntityTypes.Report),
            }
            , new()
            {
                Id = (byte)Enums.EntityTypes.Coefficient,
                Name = nameof(Enums.EntityTypes.Coefficient),
            });
        modelBuilder.Entity<Status>().HasData(
            new()
            {
                Id = (byte)Enums.Statuses.Ready,
                Name = nameof(Enums.Statuses.Ready),
                Description = "ready to compute"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.Processing,
                Name = nameof(Enums.Statuses.Processing),
                Description = "computing in process"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.Computed,
                Name = nameof(Enums.Statuses.Computed),
                Description = "computing was completed"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.NotComputed,
                Name = nameof(Enums.Statuses.NotComputed),
                Description = "computing was not done"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.Error,
                Name = nameof(Enums.Statuses.Error),
                Description = "error"
            });
    }
}