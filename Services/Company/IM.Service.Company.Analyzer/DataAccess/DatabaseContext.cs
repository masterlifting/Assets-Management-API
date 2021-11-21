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
                    Id = (byte)StatusType.ToCalculate,
                    Name = nameof(StatusType.ToCalculate),
                    Description = "before calculating"
                }
                , new()
                {
                    Id = (byte)StatusType.Calculating,
                    Name = nameof(StatusType.Calculating),
                    Description = "calculating now"
                }
                , new()
                {
                    Id = (byte)StatusType.CalculatedPartial,
                    Name = nameof(StatusType.CalculatedPartial),
                    Description = "after calculating with partial result"
                }
                , new()
                {
                    Id = (byte)StatusType.Calculated,
                    Name = nameof(StatusType.Calculated),
                    Description = "calculating done"
                }
                , new()
                {
                    Id = (byte)StatusType.Error,
                    Name = nameof(StatusType.Error),
                    Description = "error on calculating"
                });
        }
    }
}