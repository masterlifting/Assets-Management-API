using DataSetter.DataAccess.Entities.Companies;

using Microsoft.EntityFrameworkCore;

namespace DataSetter.DataAccess
{
    public class CompaniesDbContext : DbContext
    {
        public CompaniesDbContext()
        {
        }

        public CompaniesDbContext(DbContextOptions<CompaniesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Industry> Industries { get; set; } = null!;
        public virtual DbSet<Sector> Sectors { get; set; } = null!;
        public virtual DbSet<StockSplit> StockSplits { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Ticker);

                entity.HasIndex(e => e.IndustryId, "IX_Companies_IndustryId");

                entity.Property(e => e.Ticker).HasMaxLength(10);

                entity.Property(e => e.Name).HasMaxLength(300);

                entity.HasOne(d => d.Industry)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(d => d.IndustryId);
            });

            modelBuilder.Entity<Industry>(entity =>
            {
                entity.HasIndex(e => e.SectorId, "IX_Industries_SectorId");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Sector)
                    .WithMany(p => p.Industries)
                    .HasForeignKey(d => d.SectorId);
            });

            modelBuilder.Entity<Sector>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<StockSplit>(entity =>
            {
                entity.HasKey(e => new { e.CompanyTicker, e.Date });

                entity.Property(e => e.CompanyTicker).HasMaxLength(10);

                entity.Property(e => e.Date).HasColumnType("timestamp without time zone");

                entity.HasOne(d => d.CompanyTickerNavigation)
                    .WithMany(p => p.StockSplits)
                    .HasForeignKey(d => d.CompanyTicker);
            });
        }
    }
}
