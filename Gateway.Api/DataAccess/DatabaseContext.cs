using Gateway.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Gateway.Api.DataAccess
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountSummary> AccountSummaries { get; set; }
        public virtual DbSet<AccountTransaction> AccountTransactions { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }
        public virtual DbSet<BuyRecommendation> BuyRecommendations { get; set; }
        public virtual DbSet<Coefficient> Coefficients { get; set; }
        public virtual DbSet<Comission> Comissions { get; set; }
        public virtual DbSet<ComissionSummary> ComissionSummaries { get; set; }
        public virtual DbSet<ComissionType> ComissionTypes { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<CompanySummary> CompanySummaries { get; set; }
        public virtual DbSet<Currency> Currencies { get; set; }
        public virtual DbSet<Dividend> Dividends { get; set; }
        public virtual DbSet<DividendSummary> DividendSummaries { get; set; }
        public virtual DbSet<Exchange> Exchanges { get; set; }
        public virtual DbSet<ExchangeRate> ExchangeRates { get; set; }
        public virtual DbSet<ExchangeRateSummary> ExchangeRateSummaries { get; set; }
        public virtual DbSet<Industry> Industries { get; set; }
        public virtual DbSet<Isin> Isins { get; set; }
        public virtual DbSet<Lot> Lots { get; set; }
        public virtual DbSet<Price> Prices { get; set; }
        public virtual DbSet<Rating> Ratings { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<ReportSource> ReportSources { get; set; }
        public virtual DbSet<Sector> Sectors { get; set; }
        public virtual DbSet<SellRecommendation> SellRecommendations { get; set; }
        public virtual DbSet<StockTransaction> StockTransactions { get; set; }
        public virtual DbSet<Ticker> Tickers { get; set; }
        public virtual DbSet<TransactionStatus> TransactionStatuses { get; set; }
        public virtual DbSet<Weekend> Weekends { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=ServiceSettings:ConnectionStrings:Paviams");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "en_US.UTF-8");

            modelBuilder.Entity<Account>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<AccountSummary>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_AccountSummaries_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_AccountSummaries_CurrencyId");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountSummaries)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.AccountSummaries)
                    .HasForeignKey(d => d.CurrencyId);
            });

            modelBuilder.Entity<AccountTransaction>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_AccountTransactions_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_AccountTransactions_CurrencyId");

                entity.HasIndex(e => e.TransactionStatusId, "IX_AccountTransactions_TransactionStatusId");

                entity.Property(e => e.Amount).HasPrecision(18, 4);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.AccountTransactions)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.AccountTransactions)
                    .HasForeignKey(d => d.CurrencyId);

                entity.HasOne(d => d.TransactionStatus)
                    .WithMany(p => p.AccountTransactions)
                    .HasForeignKey(d => d.TransactionStatusId);
            });

            modelBuilder.Entity<AspNetRole>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                    .IsUnique();

                entity.Property(e => e.Name).HasMaxLength(256);

                entity.Property(e => e.NormalizedName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetRoleClaim>(entity =>
            {
                entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

                entity.Property(e => e.RoleId).IsRequired();

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetRoleClaims)
                    .HasForeignKey(d => d.RoleId);
            });

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique();

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEnd).HasColumnType("timestamp with time zone");

                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaim>(entity =>
            {
                entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserClaims)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserLogin>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

                entity.Property(e => e.UserId).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserLogins)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserRole>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId, "IX_AspNetUserRoles_RoleId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.RoleId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserRoles)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<AspNetUserToken>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AspNetUserTokens)
                    .HasForeignKey(d => d.UserId);
            });

            modelBuilder.Entity<BuyRecommendation>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_BuyRecommendations_CompanyId")
                    .IsUnique();

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.BuyRecommendation)
                    .HasForeignKey<BuyRecommendation>(d => d.CompanyId);
            });

            modelBuilder.Entity<Coefficient>(entity =>
            {
                entity.HasIndex(e => e.ReportId, "IX_Coefficients_ReportId")
                    .IsUnique();

                entity.Property(e => e.DebtLoad).HasPrecision(18, 2);

                entity.Property(e => e.Eps)
                    .HasPrecision(18, 2)
                    .HasColumnName("EPS");

                entity.Property(e => e.Pb)
                    .HasPrecision(18, 2)
                    .HasColumnName("PB");

                entity.Property(e => e.Pe)
                    .HasPrecision(18, 2)
                    .HasColumnName("PE");

                entity.Property(e => e.Profitability).HasPrecision(18, 2);

                entity.Property(e => e.Roa)
                    .HasPrecision(18, 2)
                    .HasColumnName("ROA");

                entity.Property(e => e.Roe)
                    .HasPrecision(18, 2)
                    .HasColumnName("ROE");

                entity.HasOne(d => d.Report)
                    .WithOne(p => p.Coefficient)
                    .HasForeignKey<Coefficient>(d => d.ReportId);
            });

            modelBuilder.Entity<Comission>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_Comissions_AccountId");

                entity.HasIndex(e => e.ComissionTypeId, "IX_Comissions_ComissionTypeId");

                entity.HasIndex(e => e.CurrencyId, "IX_Comissions_CurrencyId");

                entity.Property(e => e.Amount).HasPrecision(18, 2);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Comissions)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.ComissionType)
                    .WithMany(p => p.Comissions)
                    .HasForeignKey(d => d.ComissionTypeId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Comissions)
                    .HasForeignKey(d => d.CurrencyId);
            });

            modelBuilder.Entity<ComissionSummary>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_ComissionSummaries_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_ComissionSummaries_CurrencyId");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ComissionSummaries)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.ComissionSummaries)
                    .HasForeignKey(d => d.CurrencyId);
            });

            modelBuilder.Entity<ComissionType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasIndex(e => e.IndustryId, "IX_Companies_IndustryId");

                entity.HasIndex(e => e.SectorId, "IX_Companies_SectorId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.Industry)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(d => d.IndustryId);

                entity.HasOne(d => d.Sector)
                    .WithMany(p => p.Companies)
                    .HasForeignKey(d => d.SectorId);
            });

            modelBuilder.Entity<CompanySummary>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_CompanySummaries_AccountId");

                entity.HasIndex(e => e.CompanyId, "IX_CompanySummaries_CompanyId");

                entity.HasIndex(e => e.CurrencyId, "IX_CompanySummaries_CurrencyId");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.CompanySummaries)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.CompanySummaries)
                    .HasForeignKey(d => d.CompanyId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.CompanySummaries)
                    .HasForeignKey(d => d.CurrencyId);
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<Dividend>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_Dividends_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_Dividends_CurrencyId");

                entity.HasIndex(e => e.IsinId, "IX_Dividends_IsinId");

                entity.Property(e => e.Amount).HasPrecision(18, 4);

                entity.Property(e => e.Tax).HasPrecision(18, 4);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Dividends)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Dividends)
                    .HasForeignKey(d => d.CurrencyId);

                entity.HasOne(d => d.Isin)
                    .WithMany(p => p.Dividends)
                    .HasForeignKey(d => d.IsinId);
            });

            modelBuilder.Entity<DividendSummary>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_DividendSummaries_AccountId");

                entity.HasIndex(e => e.CompanyId, "IX_DividendSummaries_CompanyId");

                entity.HasIndex(e => e.CurrencyId, "IX_DividendSummaries_CurrencyId");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.DividendSummaries)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.DividendSummaries)
                    .HasForeignKey(d => d.CompanyId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.DividendSummaries)
                    .HasForeignKey(d => d.CurrencyId);
            });

            modelBuilder.Entity<Exchange>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_ExchangeRates_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_ExchangeRates_CurrencyId");

                entity.HasIndex(e => e.TransactionStatusId, "IX_ExchangeRates_TransactionStatusId");

                entity.Property(e => e.Rate).HasPrecision(18, 4);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ExchangeRates)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.ExchangeRates)
                    .HasForeignKey(d => d.CurrencyId);

                entity.HasOne(d => d.TransactionStatus)
                    .WithMany(p => p.ExchangeRates)
                    .HasForeignKey(d => d.TransactionStatusId);
            });

            modelBuilder.Entity<ExchangeRateSummary>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_ExchangeRateSummaries_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_ExchangeRateSummaries_CurrencyId");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.ExchangeRateSummaries)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.ExchangeRateSummaries)
                    .HasForeignKey(d => d.CurrencyId);
            });

            modelBuilder.Entity<Industry>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(300);
            });

            modelBuilder.Entity<Isin>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_Isins_CompanyId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Isins)
                    .HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasIndex(e => e.CurrencyId, "IX_Prices_CurrencyId");

                entity.HasIndex(e => e.TickerId, "IX_Prices_TickerId");

                entity.Property(e => e.Value).HasPrecision(18, 4);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Prices)
                    .HasForeignKey(d => d.CurrencyId);

                entity.HasOne(d => d.Ticker)
                    .WithMany(p => p.Prices)
                    .HasForeignKey(d => d.TickerId);
            });

            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_Ratings_CompanyId")
                    .IsUnique();

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.Rating)
                    .HasForeignKey<Rating>(d => d.CompanyId);
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_Reports_CompanyId");

                entity.Property(e => e.Assets).HasPrecision(18, 4);

                entity.Property(e => e.CashFlow).HasPrecision(18, 4);

                entity.Property(e => e.Dividends).HasPrecision(18, 4);

                entity.Property(e => e.GrossProfit).HasPrecision(18, 4);

                entity.Property(e => e.LongTermDebt).HasPrecision(18, 4);

                entity.Property(e => e.NetProfit).HasPrecision(18, 4);

                entity.Property(e => e.Obligations).HasPrecision(18, 4);

                entity.Property(e => e.Revenue).HasPrecision(18, 4);

                entity.Property(e => e.ShareCapital).HasPrecision(18, 4);

                entity.Property(e => e.Turnover).HasPrecision(18, 4);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Reports)
                    .HasForeignKey(d => d.CompanyId);
            });

            modelBuilder.Entity<ReportSource>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_ReportSources_CompanyId")
                    .IsUnique();

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.ReportSource)
                    .HasForeignKey<ReportSource>(d => d.CompanyId);
            });

            modelBuilder.Entity<Sector>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(300);
            });

            modelBuilder.Entity<SellRecommendation>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_SellRecommendations_CompanyId")
                    .IsUnique();

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.SellRecommendation)
                    .HasForeignKey<SellRecommendation>(d => d.CompanyId);
            });

            modelBuilder.Entity<StockTransaction>(entity =>
            {
                entity.HasIndex(e => e.AccountId, "IX_StockTransactions_AccountId");

                entity.HasIndex(e => e.CurrencyId, "IX_StockTransactions_CurrencyId");

                entity.HasIndex(e => e.ExchangeId, "IX_StockTransactions_ExchangeId");

                entity.HasIndex(e => e.TickerId, "IX_StockTransactions_TickerId");

                entity.HasIndex(e => e.TransactionStatusId, "IX_StockTransactions_TransactionStatusId");

                entity.Property(e => e.Cost).HasPrecision(18, 4);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(d => d.AccountId);

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(d => d.CurrencyId);

                entity.HasOne(d => d.Exchange)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(d => d.ExchangeId);

                entity.HasOne(d => d.Ticker)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(d => d.TickerId)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.TransactionStatus)
                    .WithMany(p => p.StockTransactions)
                    .HasForeignKey(d => d.TransactionStatusId);
            });

            modelBuilder.Entity<Ticker>(entity =>
            {
                entity.HasIndex(e => e.CompanyId, "IX_Tickers_CompanyId");

                entity.HasIndex(e => e.ExchangeId, "IX_Tickers_ExchangeId");

                entity.HasIndex(e => e.LotId, "IX_Tickers_LotId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Tickers)
                    .HasForeignKey(d => d.CompanyId);

                entity.HasOne(d => d.Exchange)
                    .WithMany(p => p.Tickers)
                    .HasForeignKey(d => d.ExchangeId);

                entity.HasOne(d => d.Lot)
                    .WithMany(p => p.Tickers)
                    .HasForeignKey(d => d.LotId);
            });

            modelBuilder.Entity<TransactionStatus>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Weekend>(entity =>
            {
                entity.HasIndex(e => e.ExchangeId, "IX_Weekends_ExchangeId");

                entity.HasOne(d => d.Exchange)
                    .WithMany(p => p.Weekends)
                    .HasForeignKey(d => d.ExchangeId);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
