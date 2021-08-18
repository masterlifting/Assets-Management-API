using IM.Services.Companies.Reports.Api.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Services.Companies.Reports.Api.DataAccess
{
    public class ReportsContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportSource> ReportSources { get; set; }
        public DbSet<ReportSourceType> ReportSourceTypes { get; set; }

        public ReportsContext(DbContextOptions<ReportsContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Report>().HasKey(x => new { x.ReportSourceId, x.Year, x.Quarter });
            modelBuilder.Entity<ReportSource>().HasOne(x => x.Ticker).WithMany(x => x.ReportSources).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ReportSource>().HasIndex(x => x.IsActive);
            modelBuilder.Entity<ReportSourceType>().Property(x => x.Id).ValueGeneratedNever();
            modelBuilder.Entity<ReportSourceType>().HasData(new ReportSourceType[]
            {
                new (){Id = 1, Name = "official" },
                new (){Id = 2, Name = "investing" }
            });
        }
    }
}