using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Catalogs;
using IM.Service.Market.Domain.Entities.ManyToMany;

using Microsoft.EntityFrameworkCore;

using SharedEnums = IM.Service.Shared.Enums;
using MarketEnums = IM.Service.Market.Enums;

namespace IM.Service.Market.Domain.DataAccess;

public class DatabaseContext : DbContext
{
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Source> Sources { get; set; } = null!;
    public DbSet<CompanySource> CompanySources { get; set; } = null!;

    public DbSet<Industry> Industries { get; set; } = null!;
    public DbSet<Sector> Sectors { get; set; } = null!;
    public DbSet<Country>Countries { get; set; } = null!;
    public DbSet<Status> Statuses { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;

    public DbSet<Price> Prices { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Coefficient> Coefficients { get; set; } = null!;
    public DbSet<Dividend> Dividends { get; set; } = null!;
    public DbSet<Split> Splits { get; set; } = null!;
    public DbSet<Float> Floats { get; set; } = null!;

    public DbSet<Rating> Rating { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<Rating>().HasKey(x => x.CompanyId);

        modelBuilder.Entity<Company>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Industry>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<Sector>().HasIndex(x => x.Name).IsUnique();

        modelBuilder.Entity<CompanySource>().HasKey(x => new {x.CompanyId, x.SourceId});
        modelBuilder.Entity<CompanySource>()
            .HasOne(x => x.Company)
            .WithMany(x => x.Sources)
            .HasForeignKey(x => x.CompanyId);
        modelBuilder.Entity<CompanySource>()
            .HasOne(x => x.Source)
            .WithMany(x => x.CompanySources)
            .HasForeignKey(x => x.SourceId);

        modelBuilder.Entity<Price>().HasKey(x => new {x.CompanyId, x.SourceId, x.Date });
        modelBuilder.Entity<Split>().HasKey(x => new {x.CompanyId, x.SourceId, x.Date });
        modelBuilder.Entity<Float>().HasKey(x => new {x.CompanyId, x.SourceId, x.Date });
        modelBuilder.Entity<Dividend>().HasKey(x => new { x.CompanyId, x.SourceId, x.Date });
        modelBuilder.Entity<Report>().HasKey(x => new {x.CompanyId, x.SourceId, x.Year, x.Quarter });
        modelBuilder.Entity<Coefficient>().HasKey(x => new { x.CompanyId, x.SourceId, x.Year, x.Quarter });

        modelBuilder.Entity<Price>().HasOne(x => x.Company).WithMany(x => x.Prices).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Split>().HasOne(x => x.Company).WithMany(x => x.Splits).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Float>().HasOne(x => x.Company).WithMany(x => x.Floats).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Dividend>().HasOne(x => x.Company).WithMany(x => x.Dividends).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Report>().HasOne(x => x.Company).WithMany(x => x.Reports).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Coefficient>().HasOne(x => x.Company).WithMany(x => x.Coefficients).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Source>().HasData(
            new() { Id = (byte)MarketEnums.Sources.Manual, Name = nameof(MarketEnums.Sources.Manual), Description = "Data from manual enter" },
            new() { Id = (byte)MarketEnums.Sources.Moex, Name = nameof(MarketEnums.Sources.Moex), Description = "Data from Moscow Exchange" },
            new() { Id = (byte)MarketEnums.Sources.Spbex, Name = nameof(MarketEnums.Sources.Spbex), Description = "Data from Spb Exchange" },
            new() { Id = (byte)MarketEnums.Sources.Yahoo, Name = nameof(MarketEnums.Sources.Yahoo), Description = "Data from YahooFinance.com" },
            new() { Id = (byte)MarketEnums.Sources.Tdameritrade, Name = nameof(MarketEnums.Sources.Tdameritrade), Description = "Data from Tdameritrade.com" },
            new() { Id = (byte)MarketEnums.Sources.Investing, Name = nameof(MarketEnums.Sources.Investing), Description = "Data from Investing.com" });

        modelBuilder.Entity<Country>().HasData(
            new() { Id = (byte)SharedEnums.Countries.Rus, Name = nameof(SharedEnums.Countries.Rus), Description = "The Russian Federation" },
            new() { Id = (byte)SharedEnums.Countries.Usa, Name = nameof(SharedEnums.Countries.Usa), Description = "Соединенные Штаты Америки" },
            new() { Id = (byte)SharedEnums.Countries.Chn, Name = nameof(SharedEnums.Countries.Chn), Description = "Chinese People's Republic" },
            new() { Id = (byte)SharedEnums.Countries.Deu, Name = nameof(SharedEnums.Countries.Deu), Description = "Deutschland" },
            new() { Id = (byte)SharedEnums.Countries.Gbr, Name = nameof(SharedEnums.Countries.Gbr), Description = "Great Britain" });

        modelBuilder.Entity<Currency>().HasData(
            new() { Id = (byte)SharedEnums.Currencies.Rub, Name = "₽", Description = nameof(SharedEnums.Currencies.Rub) },
            new() { Id = (byte)SharedEnums.Currencies.Usd, Name = "$", Description = nameof(SharedEnums.Currencies.Usd) },
            new() { Id = (byte)SharedEnums.Currencies.Eur, Name = "€", Description = nameof(SharedEnums.Currencies.Eur) },
            new() { Id = (byte)SharedEnums.Currencies.Gbp, Name = "£", Description = nameof(SharedEnums.Currencies.Gbp) },
            new() { Id = (byte)SharedEnums.Currencies.Chy, Name = "¥", Description = nameof(SharedEnums.Currencies.Chy) },
            new() { Id = (byte)SharedEnums.Currencies.Btc, Name = "₿", Description = nameof(SharedEnums.Currencies.Btc) },
            new() { Id = (byte)SharedEnums.Currencies.Eth, Name = "Ξ", Description = nameof(SharedEnums.Currencies.Eth) });

        modelBuilder.Entity<Status>().HasData(
            new() { Id = (byte)MarketEnums.Statuses.New, Name = nameof(MarketEnums.Statuses.New), Description = "new object" }
            , new() { Id = (byte)MarketEnums.Statuses.Ready, Name = nameof(MarketEnums.Statuses.Ready), Description = "ready to compute" }
            , new() { Id = (byte)MarketEnums.Statuses.Computing, Name = nameof(MarketEnums.Statuses.Computing), Description = "computing in process" }
            , new() { Id = (byte)MarketEnums.Statuses.Computed, Name = nameof(MarketEnums.Statuses.Computed), Description = "computing was completed" }
            , new() { Id = (byte)MarketEnums.Statuses.NotComputed, Name = nameof(MarketEnums.Statuses.NotComputed), Description = "computing was not done" }
            , new() { Id = (byte)MarketEnums.Statuses.Error, Name = nameof(MarketEnums.Statuses.Error), Description = "error" });

        modelBuilder.Entity<Sector>().HasData(
             new() { Id = 1, Name = "Сырье" }
            , new() { Id = 2, Name = "Средства производства" }
            , new() { Id = 3, Name = "Технологии" }
            , new() { Id = 4, Name = "Коммунальные услуги" }
            , new() { Id = 5, Name = "Энергетика" }
            , new() { Id = 6, Name = "Цикличные компании" }
            , new() { Id = 7, Name = "Финансы" }
            , new() { Id = 8, Name = "Нецикличные компании" }
            , new() { Id = 9, Name = "Здравоохранение" }
            , new() { Id = 10, Name = "Услуги" }
            , new() { Id = 11, Name = "Транспорт" });

        modelBuilder.Entity<Industry>().HasData(
             new() { Id = 20, SectorId = 1, Name = "Разные промышленные товары" }
            , new() { Id = 18, SectorId = 1, Name = "Химическое производство" }
            , new() { Id = 7, SectorId = 1, Name = "Золото и серебро" }
            , new() { Id = 29, SectorId = 1, Name = "Металлодобывающая промышленность" }
            , new() { Id = 27, SectorId = 1, Name = "Нерудная промышленность" }

            , new() { Id = 12, SectorId = 2, Name = "Строительство-снабжение" }
            , new() { Id = 8, SectorId = 2, Name = "Аэрокосмическая и оборонная промышленность" }
            , new() { Id = 22, SectorId = 2, Name = "Различные средства производства" }

            , new() { Id = 14, SectorId = 3, Name = "Научно-техническое приборостроение" }
            , new() { Id = 11, SectorId = 3, Name = "Компьютерные услуги" }
            , new() { Id = 21, SectorId = 3, Name = "Программное обеспечение и программирование" }
            , new() { Id = 19, SectorId = 3, Name = "Коммуникационное оборудование" }
            , new() { Id = 3, SectorId = 3, Name = "Полупроводники" }

            , new() { Id = 16, SectorId = 4, Name = "Электроэнергетика" }
            , new() { Id = 15, SectorId = 4, Name = "Газоснабжение" }

            , new() { Id = 30, SectorId = 5, Name = "Нефтегазовая промышленность" }
            , new() { Id = 4, SectorId = 5, Name = "Интегрированная нефтегазовая промышленность" }

            , new() { Id = 23, SectorId = 6, Name = "Автомобильная промышленность" }

            , new() { Id = 17, SectorId = 7, Name = "Региональные банки" }
            , new() { Id = 6, SectorId = 7, Name = "Потребительские финансовые услуги" }

            , new() { Id = 1, SectorId = 8, Name = "Пищевая промышленность" }
            , new() { Id = 13, SectorId = 8, Name = "Напитки" }

            , new() { Id = 5, SectorId = 9, Name = "Производство и поставки медицинского оборудования" }
            , new() { Id = 10, SectorId = 9, Name = "Биотехнологии и лекарства" }

            , new() { Id = 25, SectorId = 10, Name = "Услуги связи" }
            , new() { Id = 26, SectorId = 10, Name = "Розничная торговля" }
            , new() { Id = 9, SectorId = 10, Name = "Эфирное и кабельное телевидение" }
            , new() { Id = 24, SectorId = 10, Name = "Деловые услуги" }
            , new() { Id = 2, SectorId = 10, Name = "Отдых" }

            , new() { Id = 28, SectorId = 11, Name = "Воздушные перевозки" });

        base.OnModelCreating(modelBuilder);
    }
}