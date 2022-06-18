using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using IM.Service.Portfolio.Domain.Entities.ManyToMany;

using Microsoft.EntityFrameworkCore;

using SharedEnums = IM.Service.Shared.Enums;
using PortfolioEnums = IM.Service.Portfolio.Enums;


namespace IM.Service.Portfolio.Domain.DataAccess;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Broker> Brokers { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<EventType> EventTypes { get; set; } = null!;
    public DbSet<Exchange> Exchanges { get; set; } = null!;
    public DbSet<Operation> Operations { get; set; } = null!;
    public DbSet<UnderlyingAssetType> UnderlyingAssetTypes { get; set; } = null!;

    public DbSet<BrokerExchange> BrokerExchanges { get; set; } = null!;

    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Derivative> Derivatives { get; set; } = null!;
    public DbSet<Deal> Deals { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<UnderlyingAsset> UnderlyingAssets { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<Account>().HasIndex(x => new {Id = x.Name, x.UserId, x.BrokerId }).IsUnique();
        modelBuilder.Entity<Report>().HasKey(x => new {x.Id, x.BrokerId, x.UserId });
        modelBuilder.Entity<Derivative>().HasKey(x => new { x.Id, x.Code });


        modelBuilder.Entity<BrokerExchange>().HasKey(x => new { x.BrokerId, x.ExchangeId });
        modelBuilder.Entity<BrokerExchange>()
            .HasOne(x => x.Broker)
            .WithMany(x => x.BrokerExchanges)
            .HasForeignKey(x => x.BrokerId);
        modelBuilder.Entity<BrokerExchange>()
            .HasOne(x => x.Exchange)
            .WithMany(x => x.BrokerExchanges)
            .HasForeignKey(x => x.ExchangeId);

        modelBuilder.Entity<Broker>().HasData(
            new() { Id = (byte)PortfolioEnums.Brokers.Bcs, Name = nameof(PortfolioEnums.Brokers.Bcs) },
            new() { Id = (byte)PortfolioEnums.Brokers.Tinkoff, Name = nameof(PortfolioEnums.Brokers.Tinkoff) });

        modelBuilder.Entity<Exchange>().HasData(
            new() { Id = (byte)SharedEnums.Exchanges.Nasdaq, Name = nameof(SharedEnums.Exchanges.Nasdaq) },
            new() { Id = (byte)SharedEnums.Exchanges.Nyse, Name = nameof(SharedEnums.Exchanges.Nyse) },
            new() { Id = (byte)SharedEnums.Exchanges.Fwb, Name = nameof(SharedEnums.Exchanges.Fwb) },
            new() { Id = (byte)SharedEnums.Exchanges.Hkse, Name = nameof(SharedEnums.Exchanges.Hkse) },
            new() { Id = (byte)SharedEnums.Exchanges.Lse, Name = nameof(SharedEnums.Exchanges.Lse) },
            new() { Id = (byte)SharedEnums.Exchanges.Sse, Name = nameof(SharedEnums.Exchanges.Sse) },
            new() { Id = (byte)SharedEnums.Exchanges.Spbex, Name = nameof(SharedEnums.Exchanges.Spbex) },
            new() { Id = (byte)SharedEnums.Exchanges.Moex, Name = nameof(SharedEnums.Exchanges.Moex) });

        modelBuilder.Entity<BrokerExchange>().HasData(
            new() { BrokerId = (byte)PortfolioEnums.Brokers.Bcs, ExchangeId = (byte)SharedEnums.Exchanges.Spbex },
            new() { BrokerId = (byte)PortfolioEnums.Brokers.Bcs, ExchangeId = (byte)SharedEnums.Exchanges.Moex },
            new() { BrokerId = (byte)PortfolioEnums.Brokers.Tinkoff, ExchangeId = (byte)SharedEnums.Exchanges.Spbex },
            new() { BrokerId = (byte)PortfolioEnums.Brokers.Tinkoff, ExchangeId = (byte)SharedEnums.Exchanges.Moex });


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


        modelBuilder.Entity<Operation>().HasData(
            new() { Id = (byte)PortfolioEnums.OperationTypes.Приход, Name = nameof(PortfolioEnums.OperationTypes.Приход) },
            new() { Id = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.OperationTypes.Расход) });

        modelBuilder.Entity<UnderlyingAssetType>().HasData(
            new() { Id = (byte)PortfolioEnums.UnderlyingAssetTypes.Stock, Name = nameof(PortfolioEnums.UnderlyingAssetTypes.Stock) },
            new() { Id = (byte)PortfolioEnums.UnderlyingAssetTypes.Bond, Name = nameof(PortfolioEnums.UnderlyingAssetTypes.Bond) },
            new() { Id = (byte)PortfolioEnums.UnderlyingAssetTypes.ETF, Name = nameof(PortfolioEnums.UnderlyingAssetTypes.ETF) },
            new() { Id = (byte)PortfolioEnums.UnderlyingAssetTypes.Currency, Name = nameof(PortfolioEnums.UnderlyingAssetTypes.Currency) },
            new() { Id = (byte)PortfolioEnums.UnderlyingAssetTypes.CryptoCurrency, Name = nameof(PortfolioEnums.UnderlyingAssetTypes.CryptoCurrency) });

        modelBuilder.Entity<EventType>().HasData(
            new() { Id = (byte)PortfolioEnums.EventTypes.Пополнение_счета, OperationId = (byte)PortfolioEnums.OperationTypes.Приход, Name = nameof(PortfolioEnums.EventTypes.Пополнение_счета) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Дополнительный_выпуск_акции, OperationId = (byte)PortfolioEnums.OperationTypes.Приход, Name = nameof(PortfolioEnums.EventTypes.Дополнительный_выпуск_акции) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Дивиденд, OperationId = (byte)PortfolioEnums.OperationTypes.Приход, Name = nameof(PortfolioEnums.EventTypes.Дивиденд) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Вывод_с_счета, OperationId = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.EventTypes.Вывод_с_счета) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Делистинг_акции, OperationId = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.EventTypes.Делистинг_акции) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Налог_с_дивиденда, OperationId = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.EventTypes.Налог_с_дивиденда) },
            new() { Id = (byte)PortfolioEnums.EventTypes.НДФЛ, OperationId = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.EventTypes.НДФЛ) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Комиссия_брокера, OperationId = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.EventTypes.Комиссия_брокера) },
            new() { Id = (byte)PortfolioEnums.EventTypes.Комиссия_депозитария, OperationId = (byte)PortfolioEnums.OperationTypes.Расход, Name = nameof(PortfolioEnums.EventTypes.Комиссия_депозитария) });

        base.OnModelCreating(modelBuilder);
    }
}
