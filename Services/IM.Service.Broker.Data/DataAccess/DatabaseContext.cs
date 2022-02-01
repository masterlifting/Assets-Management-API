using IM.Service.Broker.Data.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Broker.Data.DataAccess;

public sealed class DatabaseContext : DbContext
{
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Entities.Broker> Brokers { get; set; } = null!;
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<Currency> Currencies { get; set; } = null!;
    public DbSet<Exchange> Exchanges { get; set; } = null!;
    public DbSet<Identifier> Identifiers { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<Stock> Stocks { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<TransactionAction> TransactionActions { get; set; } = null!;
    public DbSet<TransactionActionType> TransactionActionTypes { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();

        modelBuilder.Entity<Report>().HasKey(x => new { x.AccountId, FileName = x.Name });
        modelBuilder.Entity<Account>().HasIndex(x => new {x.Name, x.BrokerId, x.UserId}).IsUnique();

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
                Name = "₽",
                Description = nameof(Enums.Currencies.Rub)
            },
            new()
            {
                Id = (byte)Enums.Currencies.Usd,
                Name = "$",
                Description = nameof(Enums.Currencies.Usd)
            },
            new()
            {
                Id = (byte)Enums.Currencies.Eur,
                Name = "€",
                Description = nameof(Enums.Currencies.Eur)
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
            });

        modelBuilder.Entity<TransactionAction>().HasData(
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Пополнение_счета,
                Name = nameof(Enums.TransactionActions.Пополнение_счета).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Продажа_валюты,
                Name = nameof(Enums.TransactionActions.Продажа_валюты).ToLowerInvariant()
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
                Id = (byte)Enums.TransactionActions.Выделение_акции,
                Name = nameof(Enums.TransactionActions.Выделение_акции).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Приход,
                Id = (byte)Enums.TransactionActions.Дивиденд,
                Name = nameof(Enums.TransactionActions.Дивиденд).ToLowerInvariant()
            },
            
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Вывод_с_счета,
                Name = nameof(Enums.TransactionActions.Вывод_с_счета).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.Покупка_валюты,
                Name = nameof(Enums.TransactionActions.Покупка_валюты).ToLowerInvariant()
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
                Id = (byte)Enums.TransactionActions.Налог_с_дивиденда,
                Name = nameof(Enums.TransactionActions.Налог_с_дивиденда).ToLowerInvariant()
            },
            new()
            {
                TransactionActionTypeId = (byte)Enums.TransactionActionTypes.Расход,
                Id = (byte)Enums.TransactionActions.НДФЛ,
                Name = nameof(Enums.TransactionActions.НДФЛ).ToLowerInvariant()
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
            });

        base.OnModelCreating(modelBuilder);
    }
}
