using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RabbitMQ;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.DataAccess.Entities;
using IM.Service.Portfolio.DataAccess.Repositories;
using IM.Service.Portfolio.Models.Dto.Mq;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helpers.ServiceHelper;
using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.MqServices.Implementations;

public class RabbitSyncService : RabbitRepository, IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSyncService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public Task GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => GetCompanyResultAsync(action, data),
        QueueEntities.Companies => GetCompaniesResultAsync(action, data),
        _ => Task.CompletedTask
    };

    private async Task GetCompanyResultAsync(QueueActions action, string data)
    {
        if (!JsonHelper.TryDeserialize(data, out CompanyDto? dto))
            throw new SerializationException(nameof(CompanyDto));

        if (!Enum.TryParse<Countries>(dto!.CountryId.ToString(), true, out var country))
            throw new SerializationException(nameof(Countries));

        var client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MoexClient>();
        var response = await client.GetIsinAsync(dto.Id, country);
        var isin = response.Securities.Data[0][18].ToString();

        if (isin is null)
            throw new NullReferenceException(isin);

        var underlyingAsset = new UnderlyingAsset
        {
            Id = isin,
            UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
            CountryId = (byte)country,
            Name = dto.Name
        };

        var underlyingAssetRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<UnderlyingAsset>>();
        await GetRepositoryActionAsync(underlyingAssetRepo, action, new[] { underlyingAsset.Id }, underlyingAsset);

        var derivativeRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Derivative>>();
        var derivative = new Derivative
        {
            Id = isin,
            Code = dto.Id,
            UnderlyingAssetId = isin
        };

        await GetRepositoryActionAsync(derivativeRepo, QueueActions.Create, new[] { derivative.Id }, derivative);
    }
    private async Task GetCompaniesResultAsync(QueueActions action, string data)
    {
        if (!JsonHelper.TryDeserialize(data, out CompanyDto[]? dtos))
            throw new SerializationException(nameof(CompanyDto));

        dtos = dtos!.Where(x => Enum.TryParse<Countries>(x.CountryId.ToString(), true, out _)).ToArray();

        if (!dtos.Any())
            return;

        var client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MoexClient>();
        var underlyingAssetRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<UnderlyingAsset>>();
        var derivativeRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Derivative>>();

        foreach (var item in dtos.GroupBy(x => x.CountryId))
        {
            var country = Enum.Parse<Countries>(item.Key.ToString());
            var tickers = item.Select(x => x).ToArray();
            if (country != Countries.Rus)
                tickers = tickers.Select(x => new CompanyDto($"{x.Id}-RM", item.Key, x.Name))
                .ToArray();

            var response = await client.GetIsinsAsync(country);

            foreach (var (isin, ticker, name) in response.Securities.Data
                         .Select(x => (Ticker: x[0].ToString(), Isin: x[18].ToString()))
                         .Join(tickers,
                         x => x.Ticker,
                         y => y.Id,
                         (x, y) => (x.Isin, x.Ticker, y.Name)))
            {
                var underlyingAsset = new UnderlyingAsset
                {
                    Id = isin!,
                    UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
                    CountryId = (byte)country,
                    Name = name
                };

                await GetRepositoryActionAsync(underlyingAssetRepo, action, new[] { underlyingAsset.Id }, underlyingAsset);
                var derivative = new Derivative
                {
                    Id = isin!,
                    Code = country == Countries.Rus ? ticker! : ticker![..^3],
                    UnderlyingAssetId = isin!
                };

                await GetRepositoryActionAsync(derivativeRepo, QueueActions.Create, new[] { derivative.Id }, derivative);
            }
        }
    }
}