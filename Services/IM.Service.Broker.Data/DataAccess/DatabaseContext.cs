using IM.Service.Broker.Data.DataAccess.Entities;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Broker.Data.DataAccess;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Stock> Stocks { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Entities.Broker> Brokers { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Exchange> Exchanges { get; set; } = null!;

    public DbSet<BrokerExchange> BrokerExchanges { get; set; } = null!;
    public DbSet<BrokerUser> BrokerUsers { get; set; } = null!;
    public DbSet<CompanyExchange> CompanyExchanges { get; set; } = null!;

    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<TransactionAction> TransactionActions { get; set; } = null!;
    public DbSet<TransactionActionType> TransactionActionTypes { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<Stock>().HasIndex(x => x.Isin);

        modelBuilder.Entity<Entities.Broker>().HasData(
            new() { Id = (byte)Enums.Brokers.Bcs, Name = nameof(Enums.Brokers.Bcs).ToLowerInvariant() }
            , new() { Id = (byte)Enums.Brokers.Tinkoff, Name = nameof(Enums.Brokers.Tinkoff).ToLowerInvariant() });

        modelBuilder.Entity<Exchange>().HasData(
            new() { Id = (byte)Enums.Exchanges.Spb, Name = nameof(Enums.Exchanges.Spb).ToLowerInvariant() }
            , new() { Id = (byte)Enums.Exchanges.Moex, Name = nameof(Enums.Exchanges.Moex).ToLowerInvariant() });

        modelBuilder.Entity<Currency>().HasData(
            new()
            {
                Id = (byte)Enums.Currencies.Rub,
                Name = nameof(Enums.Currencies.Rub).ToLowerInvariant()
            },
            new()
            {
                Id = (byte)Enums.Currencies.Usd,
                Name = nameof(Enums.Currencies.Usd).ToLowerInvariant()
            }, 
            new()
            {
                Id = (byte)Enums.Currencies.Eur,
                Name = nameof(Enums.Currencies.Eur).ToLowerInvariant()
            });

        modelBuilder.Entity<TransactionActionType>().HasData(
            new()
            {
                Id = (byte)Enums.TransactionActionTypes.Приход,
                Name = nameof(Enums.TransactionActionTypes.Приход).ToLowerInvariant()
            },
            new()
            {
                Id = (byte)Enums.TransactionActionTypes.Расход,
                Name = nameof(Enums.TransactionActionTypes.Расход).ToLowerInvariant()
            },
            new()
            {
                Id = (byte)Enums.TransactionActionTypes.Перемещение,
                Name = nameof(Enums.TransactionActionTypes.Перемещение).ToLowerInvariant()
            });

        modelBuilder.Entity<TransactionAction>().HasData(
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Ввод_средств,
                Name = nameof(Enums.TransactionActions.Ввод_средств).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Продажа_акции,
                Name = nameof(Enums.TransactionActions.Продажа_акции).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Поступление_дивиденда,
                Name = nameof(Enums.TransactionActions.Поступление_дивиденда).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Выделение_акции,
                Name = nameof(Enums.TransactionActions.Выделение_акции).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Вывод_средств,
                Name = nameof(Enums.TransactionActions.Вывод_средств).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Покупка_акции,
                Name = nameof(Enums.TransactionActions.Покупка_акции).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Делистинг_акции,
                Name = nameof(Enums.TransactionActions.Делистинг_акции).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Удержание_налога_с_дивиденда,
                Name = nameof(Enums.TransactionActions.Удержание_налога_с_дивиденда).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Удержание_налога_НДФЛ,
                Name = nameof(Enums.TransactionActions.Удержание_налога_НДФЛ).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Комиссия_брокера,
                Name = nameof(Enums.TransactionActions.Комиссия_брокера).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Комиссия_депозитария,
                Name = nameof(Enums.TransactionActions.Комиссия_депозитария).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Перемещение,
                Id = (byte)Enums.TransactionActions.Сплит_акции,
                Name = nameof(Enums.TransactionActions.Сплит_акции).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Перемещение,
                Id = (byte)Enums.TransactionActions.Покупка_валюты,
                Name = nameof(Enums.TransactionActions.Покупка_валюты).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Перемещение,
                Id = (byte)Enums.TransactionActions.Продажа_валюты,
                Name = nameof(Enums.TransactionActions.Продажа_валюты).ToLowerInvariant()
            });

        base.OnModelCreating(modelBuilder);
    }
}
