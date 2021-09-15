using IM.Service.Company.Analyzer.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.DataAccess
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<Price> Prices { get; set; } = null!;
        public DbSet<Status> Statuses { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Report>().HasKey(x => new { x.TickerName, x.Year, x.Quarter });
            modelBuilder.Entity<Price>().HasKey(x => new { x.TickerName, x.Date });

            modelBuilder.Entity<Rating>().Property(x => x.Place).ValueGeneratedNever();
            modelBuilder.Entity<Rating>().HasOne(x => x.Ticker).WithOne(x => x.Rating).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Status>().Property(x => x.Id).ValueGeneratedNever();
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