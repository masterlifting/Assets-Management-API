
using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using static IM.Services.Companies.Reports.Api.Enums;

namespace IM.Services.Companies.Reports.Api.DataAccess
{
    public class ReportsContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<SourceType> SourceTypes { get; set; }

        public ReportsContext(DbContextOptions<ReportsContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Report>().HasKey(x => new { x.TickerName, x.Year, x.Quarter });
            modelBuilder.Entity<SourceType>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<SourceType>().HasData(new SourceType[]
            {
                new (){Id = (byte)ReportSourceTypes.Default, Name = "Select report source!" },
                new (){Id = (byte)ReportSourceTypes.Investing, Name = nameof(ReportSourceTypes.Investing) }
            });
        }
    }
}