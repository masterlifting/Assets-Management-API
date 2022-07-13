using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.DataAccess.Comparators;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.Helpers;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IM.Service.Portfolio.Services.Entity;

public class AssetService
{
    private const string serviceName = "Company synchronization";

    public ILogger<AssetService> Logger { get; }
    private readonly MoexClient client;
    private readonly Repository<Asset> assetRepo;
    private readonly Repository<Derivative> derivativeRepo;

    public AssetService(
        ILogger<AssetService> logger,
        MoexClient client,
        Repository<Asset> assetRepo,
        Repository<Derivative> derivativeRepo)
    {
        Logger = logger;
        this.client = client;
        this.assetRepo = assetRepo;
        this.derivativeRepo = derivativeRepo;
    }

    public async Task SetAsync(AssetMqDto model)
    {
        var (id, typeId, countryId, name) = model;

        if (!Enum.TryParse<Shared.Enums.Countries>(countryId.ToString(), true, out var country))
            throw new SerializationException(nameof(Shared.Enums.Countries));

        var response = await client.GetIsinAsync(id, country);
        var isinIndex = country == Shared.Enums.Countries.Rus ? 19 : 18;
        var isin = response.Securities.Data[0][isinIndex].ToString();
        switch (isin)
        {
            case null:
                throw new NullReferenceException(isin);
            case "0":
                throw new ArgumentException($"ISIN for {name} not recognized", isin);
        }

        var asset = new Asset
        {
            Id = id,
            TypeId = typeId,
            CountryId = countryId,
            Name = name,
        };
        await assetRepo.CreateUpdateAsync(new object[] { asset.Id, asset.TypeId }, asset, isin);

        var derivative = new Derivative
        {
            Id = isin,
            Code = id,
            AssetId = asset.Id,
            AssetTypeId = asset.TypeId
        };
        await derivativeRepo.CreateUpdateAsync(new object[] { derivative.Id, derivative.Code }, derivative, isin);
    }
    public async Task SetAsync(AssetMqDto[] models)
    {
        models = models.Where(x => Enum.TryParse<Shared.Enums.Countries>(x.CountryId.ToString(), true, out _)).ToArray();

        var assets = models.Select(x => new Asset
            {
                Id = x.Id,
                TypeId = x.TypeId,
                CountryId = x.CountryId,
                Name = x.Name
            })
            .ToArray();

        await assetRepo.CreateUpdateRangeAsync(assets, new AssetComparer(), serviceName);

        if (!models.Any())
            return;

        foreach (var group in models.GroupBy(x => x.CountryId))
        {
            var country = Enum.Parse<Shared.Enums.Countries>(group.Key.ToString());
            var tickers = group.Select(x => new { AssetId = x.Id, Ticker = x.Id, CountryId = group.Key, AssetName = x.Name }).ToArray();
                
            if (country != Shared.Enums.Countries.Rus)
                tickers = tickers.Select(x => x with {Ticker = $"{x.Ticker}-RM"}).ToArray();

            var isinIndex = country == Shared.Enums.Countries.Rus ? 19 : 18;

            var response = await client.GetIsinsAsync(country);

            foreach (var (isin, ticker, assetId) in response.Securities.Data.Select(x => (Ticker: x[0].ToString(), Isin: x[isinIndex].ToString())).Join(tickers,
                         x => x.Ticker,
                         y => y.Ticker,
                         (x, y) => (x.Isin, x.Ticker, y.AssetId)))
            {
                if (isin!.Equals("0"))
                {
                    Logger.LogWarning(serviceName, $"ISIN for {assetId} not recognized", isin);
                    continue;
                }

                var derivative = new Derivative
                {
                    Id = isin,
                    Code = assetId,
                    AssetId = assetId,
                    AssetTypeId = (byte)Shared.Enums.AssetTypes.Stock
                };

                await derivativeRepo.CreateUpdateAsync(new object[] { derivative.Id, derivative.Code }, derivative, $"{ticker} - {isin}");
            }
        }
    }
}