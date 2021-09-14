using IM.Service.Company.Reports.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

using static IM.Service.Company.Reports.Enums;

namespace IM.Service.Company.Reports.DataAccess
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SourceType> SourceTypes { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Report>().HasKey(x => new { x.TickerName, x.Year, x.Quarter });
            modelBuilder.Entity<SourceType>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<SourceType>().HasData(
                new() { Id = (byte)Enums.ReportSourceTypes.Default, Name = "Select report source!" },
                new() { Id = (byte)Enums.ReportSourceTypes.Investing, Name = nameof(Enums.ReportSourceTypes.Investing) }
            );
        }
    }
}