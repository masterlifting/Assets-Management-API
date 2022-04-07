using DataSetter.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace DataSetter.DataAccess
{
    public partial class CompanyDataContext : DbContext
    {
        public CompanyDataContext()
        {
        }

        public CompanyDataContext(DbContextOptions<CompanyDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<CompanySource> CompanySources { get; set; } = null!;
        public virtual DbSet<Industry> Industries { get; set; } = null!;
        public virtual DbSet<Price> Prices { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<Sector> Sectors { get; set; } = null!;
        public virtual DbSet<Source> Sources { get; set; } = null!;
        public virtual DbSet<StockSplit> StockSplits { get; set; } = null!;
        public virtual DbSet<StockVolume> StockVolumes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=ServiceSettings:ConnectionStrings:Paviams");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.IndustryId, "IX_Companies_IndustryId");

                entity.HasIndex(e => e.Name, "IX_Companies_Name")
                    .IsUnique();

                entity.Property(e => e.Id).HasMaxLength(10);

                entity.Property(e => e.Description).HasMaxLength(300);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Industry)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(d => d.IndustryId);
            });

            modelBuilder.Entity<CompanySource>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.SourceId });

                entity.HasIndex(e => e.SourceId, "IX_CompanySources_SourceId");

                entity.Property(e => e.CompanyId).HasMaxLength(10);

                entity.Property(e => e.Value).HasMaxLength(300);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanySources)
                    .HasForeignKey(d => d.CompanyId);

                entity.HasOne(d => d.Source)
                    .WithMany(p => p.CompanySources)
                    .HasForeignKey(d => d.SourceId);
            });

            modelBuilder.Entity<Industry>(entity =>
            {
                entity.HasIndex(e => e.Name, "IX_Industries_Name")
                    .IsUnique();

                entity.HasIndex(e => e.SectorId, "IX_Industries_SectorId");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.Sector)
                    .WithMany(p => p.Industries)
                    .HasForeignKey(d => d.SectorId);
            });

            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.Date });

                entity.Property(e => e.CompanyId).HasMaxLength(10);

                entity.Property(e => e.SourceType).HasMaxLength(50);

                entity.Property(e => e.Value).HasPrecision(18, 4);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Prices)
                    .HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.Year, e.Quarter });

                entity.Property(e => e.CompanyId).HasMaxLength(10);

                entity.Property(e => e.Asset).HasPrecision(18, 4);

                entity.Property(e => e.CashFlow).HasPrecision(18, 4);

                entity.Property(e => e.LongTermDebt).HasPrecision(18, 4);

                entity.Property(e => e.Obligation).HasPrecision(18, 4);

                entity.Property(e => e.ProfitGross).HasPrecision(18, 4);

                entity.Property(e => e.ProfitNet).HasPrecision(18, 4);

                entity.Property(e => e.Revenue).HasPrecision(18, 4);

                entity.Property(e => e.ShareCapital).HasPrecision(18, 4);

                entity.Property(e => e.SourceType).HasMaxLength(50);

                entity.Property(e => e.Turnover).HasPrecision(18, 4);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<Sector>(entity =>
            {
                entity.HasIndex(e => e.Name, "IX_Sectors_Name")
                    .IsUnique();

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Source>(entity =>
            {
                entity.HasIndex(e => e.Name, "IX_Sources_Name")
                    .IsUnique();

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<StockSplit>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.Date });

                entity.Property(e => e.CompanyId).HasMaxLength(10);

                entity.Property(e => e.SourceType).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.StockSplits)
                    .HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<StockVolume>(entity =>
            {
                entity.HasKey(e => new { e.CompanyId, e.Date });

                entity.Property(e => e.CompanyId).HasMaxLength(10);

                entity.Property(e => e.SourceType).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.StockVolumes)
                    .HasForeignKey(d => d.CompanyId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
