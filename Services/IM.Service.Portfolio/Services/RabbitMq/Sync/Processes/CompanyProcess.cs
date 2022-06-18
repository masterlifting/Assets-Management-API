using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using IM.Service.Shared.Helpers;
using IM.Service.Shared.RabbitMq;
using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.DataAccess.Comparators;
using IM.Service.Portfolio.Domain.Entities;

using Microsoft.Extensions.Logging;

using static IM.Service.Shared.Enums;
using static IM.Service.Portfolio.Enums;
using IM.Service.Shared.Models.RabbitMq.Api;

namespace IM.Service.Portfolio.Services.RabbitMq.Sync.Processes;

public class CompanyProcess : IRabbitProcess
{
    private const string serviceName = "Company synchronization";

    private readonly ILogger<CompanyProcess> logger;
    private readonly MoexClient client;
    private readonly Repository<UnderlyingAsset> underlyingAssetRepo;
    private readonly Repository<Derivative> derivativeRepo;

    public CompanyProcess(
        ILogger<CompanyProcess> logger,
        MoexClient client,
        Repository<UnderlyingAsset> underlyingAssetRepo,
        Repository<Derivative> derivativeRepo)
    {
        this.logger = logger;
        this.client = client;
        this.underlyingAssetRepo = underlyingAssetRepo;
        this.derivativeRepo = derivativeRepo;
    }

    public Task ProcessAsync<T>(QueueActions action, T model) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Set => model switch
        {
            CompanyMqDto company => SetAsync(company),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };
    public Task ProcessRangeAsync<T>(QueueActions action, IEnumerable<T> models) where T : class => action switch
    {
        QueueActions.Create or QueueActions.Update or QueueActions.Set => models switch
        {
            CompanyMqDto[] companies => SetAsync(companies),
            _ => Task.CompletedTask
        },
        _ => Task.CompletedTask
    };

    private async Task SetAsync(CompanyMqDto model)
    {
        var (id, countryId, name) = model;

        if (!Enum.TryParse<Countries>(countryId.ToString(), true, out var country))
            throw new SerializationException(nameof(Countries));

        var response = await client.GetIsinAsync(id, country);
        var isinIndex = country == Countries.Rus ? 19 : 18;
        var isin = response.Securities.Data[0][isinIndex].ToString();
        switch (isin)
        {
            case null:
                throw new NullReferenceException(isin);
            case "0":
                throw new ArgumentException($"ISIN for {name} not recognized", isin);
        }

        var underlyingAsset = new UnderlyingAsset
        {
            Id = id,
            CountryId = countryId,
            Name = name,
            UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock
        };
        await underlyingAssetRepo.CreateUpdateAsync(new object[] { underlyingAsset.Id }, underlyingAsset, isin);

        var derivative = new Derivative
        {
            Id = isin,
            Code = id,
            UnderlyingAssetId = underlyingAsset.Id
        };
        await derivativeRepo.CreateUpdateAsync(new object[] { derivative.Id, derivative.Code }, derivative, isin);

        logger.LogDebug(serviceName, isin, "OK");
    }
    private async Task SetAsync(CompanyMqDto[] models)
    {
        models = models.Where(x => Enum.TryParse<Countries>(x.CountryId.ToString(), true, out _)).ToArray();

        var underlyingAssets = models.Select(x => new UnderlyingAsset
        {
            Id = x.Id,
            UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
            CountryId = x.CountryId,
            Name = x.Name
        })
        .ToArray();

        await underlyingAssetRepo.CreateUpdateRangeAsync(underlyingAssets, new UnderlyingAssetComparer(), serviceName);

        if (!models.Any())
            return;

        foreach (var group in models.GroupBy(x => x.CountryId))
        {
            var country = Enum.Parse<Countries>(group.Key.ToString());
            var tickers = group.Select(x => new { CompanyId = x.Id, Ticker = x.Id, CountryId = group.Key, CompanyName = x.Name }).ToArray();
            if (country != Countries.Rus)
                tickers = tickers.Select(x => new { x.CompanyId, Ticker = $"{x.Ticker}-RM", x.CountryId, x.CompanyName }).ToArray();

            var isinIndex = country == Countries.Rus ? 19 : 18;

            var response = await client.GetIsinsAsync(country);

            foreach (var (isin, ticker, companyId) in response.Securities.Data
                         .Select(x => (Ticker: x[0].ToString(), Isin: x[isinIndex].ToString()))
                         .Join(tickers,
                             x => x.Ticker,
                             y => y.Ticker,
                             (x, y) => (x.Isin, x.Ticker, y.CompanyId)))
            {
                if (isin!.Equals("0"))
                {
                    logger.LogWarning(serviceName, $"ISIN for {companyId} not recognized", isin);
                    continue;
                }

                var derivative = new Derivative
                {
                    Id = isin,
                    Code = companyId,
                    UnderlyingAssetId = companyId
                };
                await derivativeRepo.CreateUpdateAsync(new object[] { derivative.Id, derivative.Code }, derivative, $"{ticker} - {isin}");
            }
        }
    }
}