using DataSetter.DataAccess.Entities.Prices;

using Microsoft.EntityFrameworkCore;

namespace DataSetter.DataAccess
{
    public class PricesDbContext : DbContext
    {
        public PricesDbContext()
        {
        }

        public PricesDbContext(DbContextOptions<PricesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Price> Prices { get; set; } = null!;
        public virtual DbSet<SourceType> SourceTypes { get; set; } = null!;
        public virtual DbSet<Ticker> Tickers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasKey(e => new { e.TickerName, e.Date });

                entity.Property(e => e.TickerName).HasMaxLength(10);

                entity.Property(e => e.SourceType).HasMaxLength(50);

                entity.HasOne(d => d.TickerNameNavigation)
                    .WithMany(p => p.Prices)
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

                entity.Property(e => e.SourceValue).HasMaxLength(10);

                entity.HasOne(d => d.SourceType)
                    .WithMany(p => p.Tickers)
                    .HasForeignKey(d => d.SourceTypeId);
            });
        }
    }
}
