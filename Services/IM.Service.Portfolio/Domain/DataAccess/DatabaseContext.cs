using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Domain.Entities.Catalogs;
using IM.Service.Shared.Models.Entity;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Portfolio.Domain.DataAccess;

public sealed class DatabaseContext : DbContext
{
    public DbSet<AssetType> AssetTypes { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;

    public DbSet<Derivative> Derivatives { get; set; } = null!;

    public DbSet<Income> Incomes { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Deal> Deals { get; set; } = null!;
    public DbSet<OperationType> OperationTypes { get; set; } = null!;
    public DbSet<EventType> EventTypes { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;

    public DbSet<Report> Reports { get; set; } = null!;

    public DbSet<Provider> Providers { get; set; } = null!;
    public DbSet<Exchange> Exchanges { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        //Database.EnsureDeleted();
        //Database.EnsureCreated();
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.UseSerialColumns();

        builder.Entity<Asset>().HasKey(x => new { x.Id, x.TypeId });
        builder.Entity<AssetType>().HasData(Catalogs.AssetTypes);
        builder.Entity<Country>().HasData(Catalogs.Countries);

        builder.Entity<Exchange>().HasData(Catalogs.Exchanges);

        builder.Entity<Account>().HasIndex(x => new { x.Name, x.UserId, x.ProviderId }).IsUnique();
        builder.Entity<Report>().HasKey(x => new { x.Id, x.ProviderId, x.AccountId });
        builder.Entity<Derivative>().HasKey(x => new { x.Id, x.Code });

        builder.Entity<Provider>().HasData(
            new() { Id = (int)Enums.Providers.Default, Name = nameof(Enums.Providers.Default), Description = "Не определено" },
            new() { Id = (int)Enums.Providers.Safe, Name = nameof(Enums.Providers.Safe), Description = "Приватное хранение" },
            new() { Id = (int)Enums.Providers.Bcs, Name = "BCS", Description = "Российский брокер-банк" },
            new() { Id = (int)Enums.Providers.Tinkoff, Name = nameof(Enums.Providers.Tinkoff), Description = "Российский банк-брокер" },
            new() { Id = (int)Enums.Providers.Vtb, Name = "ВТБ", Description = "Российский банк-брокер" },
            new() { Id = (int)Enums.Providers.LedgerNanoSPlus, Name = "Ledger Nano S Plus", Description = "Aппаратный крипто кошелек" },
            new() { Id = (int)Enums.Providers.JetLend, Name = "JetLend", Description = "Российская краудлендинговая компания" }
            );

        builder.Entity<OperationType>().HasData(
            new() { Id = (byte)Enums.OperationTypes.Default, Name = nameof(Enums.OperationTypes.Default), Description = "Не определено" },
            new() { Id = (byte)Enums.OperationTypes.Income, Name = nameof(Enums.OperationTypes.Income), Description = "Приход" },
            new() { Id = (byte)Enums.OperationTypes.Expense, Name = nameof(Enums.OperationTypes.Expense), Description = "Расход" });

        builder.Entity<EventType>().HasData(
            new() { Id = (byte)Enums.EventTypes.Default, OperationTypeId = (byte)Enums.OperationTypes.Default, Name = nameof(Enums.EventTypes.Default), Description = "Не определено" },
            
            new() { Id = (byte)Enums.EventTypes.Refill, OperationTypeId = (byte)Enums.OperationTypes.Income, Name = nameof(Enums.EventTypes.Refill), Description = "Пополнение актива" },
            new() { Id = (byte)Enums.EventTypes.AddIncome, OperationTypeId = (byte)Enums.OperationTypes.Income, Name = nameof(Enums.EventTypes.AddIncome), Description = "Дополнительный доход по активу" },
            
            new() { Id = (byte)Enums.EventTypes.Withdraw, OperationTypeId = (byte)Enums.OperationTypes.Expense, Name = nameof(Enums.EventTypes.Withdraw), Description = "Опустошение актива" },
            new() { Id = (byte)Enums.EventTypes.TaxIncome, OperationTypeId = (byte)Enums.OperationTypes.Expense, Name = nameof(Enums.EventTypes.TaxIncome), Description = "Налог на доход по активу" },
            new() { Id = (byte)Enums.EventTypes.TaxPersonal, OperationTypeId = (byte)Enums.OperationTypes.Expense, Name = nameof(Enums.EventTypes.TaxPersonal), Description = "Налог на доход персональный обязательный" },
            new() { Id = (byte)Enums.EventTypes.TaxProvider, OperationTypeId = (byte)Enums.OperationTypes.Expense, Name = nameof(Enums.EventTypes.TaxProvider), Description = "Комиссия поставщику" },
            new() { Id = (byte)Enums.EventTypes.TaxThirdParty, OperationTypeId = (byte)Enums.OperationTypes.Expense, Name = nameof(Enums.EventTypes.TaxThirdParty), Description = "Комиссия третьему лицу" });

        base.OnModelCreating(builder);
    }
}
