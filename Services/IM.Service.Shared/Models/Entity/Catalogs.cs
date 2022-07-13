namespace IM.Service.Shared.Models.Entity;

public static class Catalogs
{
    public static readonly Catalog[] AssetTypes =
    {
        new() { Id = (byte) Enums.AssetTypes.Default, Name = nameof(Enums.AssetTypes.Default), Description = "Не определено" },
        new() {Id = (byte) Enums.AssetTypes.Valuable, Name = nameof(Enums.AssetTypes.Valuable), Description = "Драгоценности" },
        new() {Id = (byte) Enums.AssetTypes.Stock, Name = nameof(Enums.AssetTypes.Stock), Description = "Акции" },
        new() {Id = (byte) Enums.AssetTypes.Bond, Name = nameof(Enums.AssetTypes.Bond), Description = "Облигации" },
        new() {Id = (byte) Enums.AssetTypes.ETF, Name = nameof(Enums.AssetTypes.ETF), Description = "Фонды" },
        new() {Id = (byte) Enums.AssetTypes.Currency, Name = nameof(Enums.AssetTypes.Currency), Description = "Валюты" },
        new() {Id = (byte) Enums.AssetTypes.CryptoCurrency, Name = nameof(Enums.AssetTypes.CryptoCurrency), Description = "Криптовалюты" },
        new() {Id = (byte) Enums.AssetTypes.NFT, Name = nameof(Enums.AssetTypes.NFT), Description = "NFT"},
        new() {Id = (byte) Enums.AssetTypes.RealEstate, Name = nameof(Enums.AssetTypes.RealEstate), Description = "Недвижимое имущество"},
        new() {Id = (byte) Enums.AssetTypes.PersonalEstate, Name = nameof(Enums.AssetTypes.PersonalEstate), Description = "Движимое имущество"},
        new() {Id = (byte) Enums.AssetTypes.Crowdlending, Name = nameof(Enums.AssetTypes.Crowdlending), Description = "Краудлендинг"},
        new() {Id = (byte) Enums.AssetTypes.Crowdfunding, Name = nameof(Enums.AssetTypes.Crowdfunding), Description = "Краудфандинг"},
        new() {Id = (byte) Enums.AssetTypes.Venture, Name = nameof(Enums.AssetTypes.Venture), Description = "Венчурные инвестиции"}
    };

    public static readonly Catalog[] Exchanges =
    {
        new() {Id = (byte) Enums.Exchanges.Default, Name = nameof(Enums.Exchanges.Default) },
        new() {Id = (byte) Enums.Exchanges.Nasdaq, Name = nameof(Enums.Exchanges.Nasdaq) },
        new() {Id = (byte) Enums.Exchanges.Nyse, Name = nameof(Enums.Exchanges.Nyse)},
        new() {Id = (byte) Enums.Exchanges.Fwb, Name = nameof(Enums.Exchanges.Fwb)},
        new() {Id = (byte) Enums.Exchanges.Hkse, Name = nameof(Enums.Exchanges.Hkse)},
        new() {Id = (byte) Enums.Exchanges.Lse, Name = nameof(Enums.Exchanges.Lse)},
        new() {Id = (byte) Enums.Exchanges.Sse, Name = nameof(Enums.Exchanges.Sse)},
        new() {Id = (byte) Enums.Exchanges.Spbex, Name = nameof(Enums.Exchanges.Spbex)},
        new() {Id = (byte) Enums.Exchanges.Moex, Name = nameof(Enums.Exchanges.Moex)},
        new() {Id = (byte) Enums.Exchanges.Binance, Name = nameof(Enums.Exchanges.Binance)},
        new() {Id = (byte) Enums.Exchanges.FTX2, Name = nameof(Enums.Exchanges.FTX2)},
        new() {Id = (byte) Enums.Exchanges.Coinbase, Name = nameof(Enums.Exchanges.Coinbase)}
    };

    public static readonly Catalog[] Currencies =
    {
        new() { Id = (byte) Enums.Currencies.Default, Name = nameof(Enums.Currencies.Default), Description = "Не определено" },
        new() {Id = (byte) Enums.Currencies.Rub, Name = "₽", Description = nameof(Enums.Currencies.Rub)},
        new() {Id = (byte) Enums.Currencies.Usd, Name = "$", Description = nameof(Enums.Currencies.Usd)},
        new() {Id = (byte) Enums.Currencies.Eur, Name = "€", Description = nameof(Enums.Currencies.Eur)},
        new() {Id = (byte) Enums.Currencies.Gbp, Name = "£", Description = nameof(Enums.Currencies.Gbp)},
        new() {Id = (byte) Enums.Currencies.Chy, Name = "¥", Description = nameof(Enums.Currencies.Chy)},
        new() {Id = (byte) Enums.Currencies.Btc, Name = "₿", Description = nameof(Enums.Currencies.Btc)},
        new() {Id = (byte) Enums.Currencies.Eth, Name = "Ξ", Description = nameof(Enums.Currencies.Eth)}
    };

    public static readonly Catalog[] Countries =
    {
        new() { Id = (byte) Enums.Countries.Default, Name = nameof(Enums.Countries.Default), Description = "Не определено" },
        new() { Id = (byte) Enums.Countries.Rus, Name = nameof(Enums.Countries.Rus), Description = "Russia" },
        new() { Id = (byte) Enums.Countries.Usa, Name = nameof(Enums.Countries.Usa), Description = "USA" },
        new() { Id = (byte) Enums.Countries.Chn, Name = nameof(Enums.Countries.Chn), Description = "China" },
        new() { Id = (byte) Enums.Countries.Deu, Name = nameof(Enums.Countries.Deu), Description = "Deutschland" },
        new() { Id = (byte) Enums.Countries.Gbr, Name = nameof(Enums.Countries.Gbr), Description = "Great Britain" },
        new() { Id = (byte) Enums.Countries.Che, Name = nameof(Enums.Countries.Che), Description = "Switzerland" },
        new() { Id = (byte) Enums.Countries.Jpn, Name = nameof(Enums.Countries.Jpn), Description = "Japan" }
    };
}