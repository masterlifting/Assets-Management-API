using IM.Service.Company.Reports.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using static IM.Service.Company.Reports.Enums;

namespace IM.Service.Company.Reports.DataAccess
{
    public sealed class DatabaseContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<SourceType> SourceTypes { get; set; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Report>().HasKey(x => new { x.TickerName, x.Year, x.Quarter });
            modelBuilder.Entity<SourceType>().HasData(
                new() { Id = (byte)ReportSourceTypes.Default, Name = "Select report source" },
                new() { Id = (byte)ReportSourceTypes.Official, Name = nameof(ReportSourceTypes.Official).ToLowerInvariant() },
                new() { Id = (byte)ReportSourceTypes.Investing, Name = nameof(ReportSourceTypes.Investing).ToLowerInvariant() }
            );
        }
    }
}