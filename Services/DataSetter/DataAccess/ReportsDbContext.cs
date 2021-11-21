using DataSetter.DataAccess.Entities.Reports;

using Microsoft.EntityFrameworkCore;

namespace DataSetter.DataAccess
{
    public class ReportsDbContext : DbContext
    {
        public ReportsDbContext()
        {
        }

        public ReportsDbContext(DbContextOptions<ReportsDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<SourceType> SourceTypes { get; set; } = null!;
        public virtual DbSet<Ticker> Tickers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => new { e.TickerName, e.Year, e.Quarter });

                entity.Property(e => e.TickerName).HasMaxLength(10);

                entity.Property(e => e.Asset).HasPrecision(18, 4);

                entity.Property(e => e.CashFlow).HasPrecision(18, 4);

                entity.Property(e => e.Dividend).HasPrecision(18, 4);

                entity.Property(e => e.LongTermDebt).HasPrecision(18, 4);

                entity.Property(e => e.Obligation).HasPrecision(18, 4);

                entity.Property(e => e.ProfitGross).HasPrecision(18, 4);

                entity.Property(e => e.ProfitNet).HasPrecision(18, 4);

                entity.Property(e => e.Revenue).HasPrecision(18, 4);

                entity.Property(e => e.ShareCapital).HasPrecision(18, 4);

                entity.Property(e => e.SourceType).HasMaxLength(50);

                entity.Property(e => e.Turnover).HasPrecision(18, 4);

                entity.HasOne(d => d.TickerNameNavigation)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.TickerName);
            });

            modelBuilder.Entity<SourceType>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Ticker>(entity =>
            {
                entity.HasKey(e => e.Name);

                entity.HasIndex(e => e.SourceTypeId, "IX_Tickers_SourceTypeId");

                entity.Property(e => e.Name).HasMaxLength(10);

                entity.Property(e => e.SourceValue).HasMaxLength(300);

                entity.HasOne(d => d.SourceType)
                    .WithMany(p => p.Tickers)
                    .HasForeignKey(d => d.SourceTypeId);
            });
        }
    }
}
