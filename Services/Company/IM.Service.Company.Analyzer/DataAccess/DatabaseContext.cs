using IM.Service.Company.Analyzer.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.DataAccess;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Entities.Company> Companies { get; set; } = null!;
    public DbSet<AnalyzedEntity> AnalyzedEntities { get; set; } = null!;
    public DbSet<AnalyzedEntityType> AnalyzedEntityTypes { get; set; } = null!;
    public DbSet<Rating> Ratings { get; set; } = null!;
    public DbSet<RatingData> RatingData { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AnalyzedEntity>().HasKey(x => new { x.CompanyId, x.AnalyzedEntityTypeId, x.Date });
        modelBuilder.Entity<AnalyzedEntity>().HasOne(x => x.Company).WithMany(x => x.AnalyzedEntities).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Rating>().Property(x => x.Place).ValueGeneratedNever();
        modelBuilder.Entity<Rating>().HasOne(x => x.Company).WithOne(x => x.Rating).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AnalyzedEntityType>().HasData(
            new()
            {
                Id = (byte)EntityTypes.Price,
                Name = nameof(EntityTypes.Price),
            }
            , new()
            {
                Id = (byte)EntityTypes.Report,
                Name = nameof(EntityTypes.Report),
            }
            , new()
            {
                Id = (byte)EntityTypes.Coefficient,
                Name = nameof(EntityTypes.Coefficient),
            });
        modelBuilder.Entity<Status>().HasData(
            new()
            {
                Id = (byte)Enums.Statuses.Ready,
                Name = nameof(Enums.Statuses.Ready),
                Description = "ready to calculate"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.Processing,
                Name = nameof(Enums.Statuses.Processing),
                Description = "calculating now"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.Completed,
                Name = nameof(Enums.Statuses.Completed),
                Description = "calculating complete"
            }
            , new()
            {
                Id = (byte)Enums.Statuses.Error,
                Name = nameof(Enums.Statuses.Error),
                Description = "error"
            });
    }
}