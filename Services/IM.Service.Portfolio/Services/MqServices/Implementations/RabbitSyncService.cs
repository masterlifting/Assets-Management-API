using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.DataAccess.Entities;
using IM.Service.Portfolio.DataAccess.Repositories;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.MqServices.Implementations;

public class RabbitSyncService : RabbitRepositoryHandler, IRabbitActionService
{
    private readonly IServiceScopeFactory scopeFactory;
    public RabbitSyncService(IServiceScopeFactory scopeFactory) => this.scopeFactory = scopeFactory;

    public async Task<bool> GetActionResultAsync(QueueEntities entity, QueueActions action, string data) => entity switch
    {
        QueueEntities.Company => await GetCompanyResultAsync(action, data),
        QueueEntities.Companies => await GetCompaniesResultAsync(action, data),
        _ => true
    };

    private async Task<bool> GetCompanyResultAsync(QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out CompanyDto? dto))
            return false;

        if (!Enum.TryParse<Countries>(dto!.Country, true, out var country))
            return false;

        var client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MoexClient>();
        var response = await client.GetIsinAsync(dto.Id, country);
        var isin = response.Securities.Data[0][18].ToString();

        if (isin is null)
            return false;

        var underlyingAsset = new UnderlyingAsset
        {
            Id = isin,
            UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
            CountryId = (byte)country,
            Name = dto.Name
        };

        var underlyingAssetRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<UnderlyingAsset>>();
        var result = await GetRepositoryActionAsync(underlyingAssetRepo, action, new[] {underlyingAsset.Id}, underlyingAsset);

        if (result)
        {
            var derivativeRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Derivative>>();
            var derivative = new Derivative
            {
                Id = isin,
                Code = dto.Id,
                UnderlyingAssetId = isin
            };
        
            result = await GetRepositoryActionAsync(derivativeRepo, QueueActions.Create, new[] { derivative.Id }, derivative);
        }

        return result;
    }
    private async Task<bool> GetCompaniesResultAsync(QueueActions action, string data)
    {
        if (!RabbitHelper.TrySerialize(data, out CompanyDto[]? dtos))
            return false;

        dtos = dtos!.Where(x => Enum.TryParse<Countries>(x.Country, true, out _)).ToArray();

        if (!dtos.Any())
            return false;

        var client = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<MoexClient>();
        var underlyingAssetRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<UnderlyingAsset>>();
        var derivativeRepo = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<Repository<Derivative>>();

        foreach (var item in dtos.GroupBy(x => x.Country))
        {
            var country = Enum.Parse<Countries>(item.Key);
            var tickers = item.Select(x => x).ToArray();
            if(country != Countries.Rus)
                tickers = tickers.Select(x => new CompanyDto
                {
                    Id = $"{x.Id}-RM",
                    Name = x.Name,
                    Country = item.Key
                })
                .ToArray();

            var response = await client.GetIsinsAsync(country);

            foreach (var (isin, ticker, name) in response.Securities.Data
                         .Select(x => (Ticker:x[0].ToString(),Isin:x[18].ToString()))
                         .Join(tickers,
                         x => x.Ticker, 
                         y => y.Id, 
                         (x,y) => (x.Isin,x.Ticker,y.Name)))
            {
                var underlyingAsset = new UnderlyingAsset
                {
                    Id = isin!,
                    UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
                    CountryId = (byte)country,
                    Name = name
                };

                var result = await GetRepositoryActionAsync(underlyingAssetRepo, action, new[] { underlyingAsset.Id }, underlyingAsset);

                if (result)
                {
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

        return true;
    }
}