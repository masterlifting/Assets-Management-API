using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using IM.Service.Portfolio.Domain.Entities.ManyToMany;
using Microsoft.EntityFrameworkCore;
using CommonEnums = IM.Service.Common.Net.Enums;

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

    public DbSet<BrokerUser> BrokerUsers { get; set; } = null!;
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

        modelBuilder.Entity<Account>().HasKey(x => new {x.UserId, x.BrokerId, x.Name});
        modelBuilder.Entity<Report>().HasKey(x => new { x.AccountUserId, x.AccountBrokerId, x.Name });
        modelBuilder.Entity<Derivative>().HasKey(x => new { x.Id, x.Code });

        modelBuilder.Entity<BrokerExchange>().HasKey(x => new {x.BrokerId, x.ExchangeId});
        modelBuilder.Entity<BrokerExchange>()
            .HasOne(x => x.Broker)
            .WithMany(x => x.BrokerExchanges)
            .HasForeignKey(x => x.BrokerId);
        modelBuilder.Entity<BrokerExchange>()
            .HasOne(x => x.Exchange)
            .WithMany(x => x.BrokerExchanges)
            .HasForeignKey(x => x.ExchangeId);

        modelBuilder.Entity<BrokerUser>().HasKey(x => new { x.BrokerId, x.UserId });
        modelBuilder.Entity<BrokerUser>()
            .HasOne(x => x.Broker)
            .WithMany(x => x.BrokerUsers)
            .HasForeignKey(x => x.BrokerId);
        modelBuilder.Entity<BrokerUser>()
            .HasOne(x => x.User)
            .WithMany(x => x.BrokerUsers)
            .HasForeignKey(x => x.UserId);

        modelBuilder.Entity<Exchange>().HasData(
            new() { Id = (byte)CommonEnums.Exchanges.Spbex, Name = nameof(CommonEnums.Exchanges.Spbex) }, 
            new() { Id = (byte)CommonEnums.Exchanges.Moex, Name = nameof(CommonEnums.Exchanges.Moex) });

        modelBuilder.Entity<Country>().HasData(
            new() { Id = (byte)CommonEnums.Countries.Rus, Name = nameof(CommonEnums.Countries.Rus) },
            new() { Id = (byte)CommonEnums.Countries.Usa, Name = nameof(CommonEnums.Countries.Usa) },
            new() { Id = (byte)CommonEnums.Countries.Chn, Name = nameof(CommonEnums.Countries.Chn) });

        modelBuilder.Entity<Currency>().HasData(
            new() {Id = (byte)CommonEnums.Currencies.Rub, Name = "₽", Description = nameof(CommonEnums.Currencies.Rub)},
            new() {Id = (byte)CommonEnums.Currencies.Usd, Name = "$", Description = nameof(CommonEnums.Currencies.Usd)},
            new() {Id = (byte)CommonEnums.Currencies.Eur, Name = "€", Description = nameof(CommonEnums.Currencies.Eur)});

        modelBuilder.Entity<Broker>().HasData(
            new() { Id = (byte)Enums.Brokers.Bcs, Name = nameof(Enums.Brokers.Bcs) },
            new() { Id = (byte)Enums.Brokers.Tinkoff, Name = nameof(Enums.Brokers.Tinkoff) });

        modelBuilder.Entity<Operation>().HasData(
            new() {Id = (byte)Enums.Operations.Приход, Name = nameof(Enums.Operations.Приход)},
            new() {Id = (byte)Enums.Operations.Расход, Name = nameof(Enums.Operations.Расход)});

        modelBuilder.Entity<UnderlyingAssetType>().HasData(
            new () { Id = (byte)Enums.UnderlyingAssetTypes.Stock, Name = nameof(Enums.UnderlyingAssetTypes.Stock)},
            new () { Id = (byte)Enums.UnderlyingAssetTypes.Bond, Name = nameof(Enums.UnderlyingAssetTypes.Bond) },
            new () { Id = (byte)Enums.UnderlyingAssetTypes.ETF, Name = nameof(Enums.UnderlyingAssetTypes.ETF) },
            new () { Id = (byte)Enums.UnderlyingAssetTypes.Currency, Name = nameof(Enums.UnderlyingAssetTypes.Currency) });

        modelBuilder.Entity<EventType>().HasData(
            new() {Id = (byte)Enums.EventTypes.Пополнение_счета, OperationId = (byte)Enums.Operations.Приход, Name = nameof(Enums.EventTypes.Пополнение_счета)},
            new() {Id = (byte)Enums.EventTypes.Дополнительный_выпуск_акции, OperationId = (byte)Enums.Operations.Приход,  Name = nameof(Enums.EventTypes.Дополнительный_выпуск_акции)},
            new() {Id = (byte)Enums.EventTypes.Дивиденд, OperationId = (byte)Enums.Operations.Приход,  Name = nameof(Enums.EventTypes.Дивиденд)},
            new() {Id = (byte)Enums.EventTypes.Вывод_с_счета, OperationId = (byte)Enums.Operations.Расход,  Name = nameof(Enums.EventTypes.Вывод_с_счета)},
            new() {Id = (byte)Enums.EventTypes.Делистинг_акции, OperationId = (byte)Enums.Operations.Расход,  Name = nameof(Enums.EventTypes.Делистинг_акции)},
            new() {Id = (byte)Enums.EventTypes.Налог_с_дивиденда, OperationId = (byte)Enums.Operations.Расход,  Name = nameof(Enums.EventTypes.Налог_с_дивиденда)},
            new() {Id = (byte)Enums.EventTypes.НДФЛ, OperationId = (byte)Enums.Operations.Расход,  Name = nameof(Enums.EventTypes.НДФЛ)},
            new() {Id = (byte)Enums.EventTypes.Комиссия_брокера, OperationId = (byte)Enums.Operations.Расход,  Name = nameof(Enums.EventTypes.Комиссия_брокера)},
            new() {Id = (byte)Enums.EventTypes.Комиссия_депозитария, OperationId = (byte)Enums.Operations.Расход,  Name = nameof(Enums.EventTypes.Комиссия_депозитария)});

        base.OnModelCreating(modelBuilder);
    }
}
