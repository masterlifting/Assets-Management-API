using IM.Services.Analyzer.Api.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace IM.Services.Analyzer.Api.DataAccess
{
    public class AnalyzerContext : DbContext
    {
        public DbSet<Ticker> Tickers { get; set; } = null!;
        public DbSet<Coefficient> Coefficients { get; set; } = null!;
        public DbSet<Rating> Ratings { get; set; } = null!;
        public DbSet<Recommendation> Recommendations { get; set; } = null!;
        public DbSet<ToBuy> ToBuy { get; set; } = null!;
        public DbSet<ToSell> ToSell { get; set; } = null!;

        public AnalyzerContext(DbContextOptions<AnalyzerContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Coefficient>().HasKey(x => new { x.ReportSource, x.Year, x.Quarter });
            modelBuilder.Entity<Rating>().Property(x => x.Place).ValueGeneratedNever();
        }
    }
}