using IM.Service.Company.Analyzer.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.DataAccess
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<Entities.Company> Companies { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Price> Prices { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Price>().HasKey(x => new { x.CompanyId, x.Date });
            modelBuilder.Entity<Report>().HasKey(x => new { x.CompanyId, x.Year, x.Quarter });

            modelBuilder.Entity<Price>().HasOne(x => x.Company).WithMany(x => x.Prices).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Report>().HasOne(x => x.Company).WithMany(x => x.Reports).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Rating>().Property(x => x.Place).ValueGeneratedNever();
            modelBuilder.Entity<Rating>().HasOne(x => x.Company).WithOne(x => x.Rating).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Status>().HasData(
                new()
                {
                    Id = (byte)StatusType.Ready,
                    Name = nameof(StatusType.Ready),
                    Description = "ready to calculate"
                }
                , new()
                {
                    Id = (byte)StatusType.Processing,
                    Name = nameof(StatusType.Processing),
                    Description = "calculating now"
                }
                , new()
                {
                    Id = (byte)StatusType.Completed,
                    Name = nameof(StatusType.Completed),
                    Description = "calculating complete"
                }
                , new()
                {
                    Id = (byte)StatusType.Error,
                    Name = nameof(StatusType.Error),
                    Description = "error"
                });
        }
    }
}