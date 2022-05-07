using IM.Service.Common.Net.Helpers;
using IM.Service.Portfolio.Clients;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;
using IM.Service.Portfolio.Models.Api.Mq;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Portfolio.Enums;

namespace IM.Service.Portfolio.Services.Data.Isins;

public class MoexIsinService
{
    private readonly ILogger<MoexIsinService> logger;
    private readonly MoexClient client;
    private readonly Repository<UnderlyingAsset> underlyingAssetRepo;
    private readonly Repository<Derivative> derivativeRepo;

    public MoexIsinService(
        ILogger<MoexIsinService> logger,
        MoexClient client,
        Repository<UnderlyingAsset> underlyingAssetRepo,
        Repository<Derivative> derivativeRepo)
    {
        this.logger = logger;
        this.client = client;
        this.underlyingAssetRepo = underlyingAssetRepo;
        this.derivativeRepo = derivativeRepo;
    }
    public async Task GetAsync(string data)
    {
        if (!JsonHelper.TryDeserialize(data, out CompanyDto? dto))
            throw new SerializationException(nameof(CompanyDto));

        if (!Enum.TryParse<Countries>(dto!.CountryId.ToString(), true, out var country))
            throw new SerializationException(nameof(Countries));

        var response = await client.GetIsinAsync(dto.Id, country);
        var isin = response.Securities.Data[0][18].ToString();

        switch (isin)
        {
            case null:
                throw new NullReferenceException(isin);
            case "0":
                throw new ArgumentException($"ISIN for {dto.Name} not recognized", isin);
        }

        var underlyingAsset = new UnderlyingAsset
        {
            Id = isin,
            UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
            CountryId = (byte)country,
            Name = dto.Name
        };
        await underlyingAssetRepo.CreateUpdateAsync(new object[] { underlyingAsset.Id }, underlyingAsset, isin);

        var derivative = new Derivative
        {
            Id = isin,
            Code = dto.Id,
            UnderlyingAssetId = isin
        };
        await derivativeRepo.CreateUpdateAsync(new object[] { derivative.Id, derivative.Code }, derivative, isin);

        logger.LogDebug(nameof(GetAsync), isin, "Ok");
    }
    public async Task GetRangeAsync(string data)
    {
        if (!JsonHelper.TryDeserialize(data, out CompanyDto[]? dtos))
            throw new SerializationException(nameof(CompanyDto));

        dtos = dtos!.Where(x => Enum.TryParse<Countries>(x.CountryId.ToString(), true, out _)).ToArray();

        if (!dtos.Any())
            return;

        foreach (var group in dtos.GroupBy(x => x.CountryId))
        {
            var country = Enum.Parse<Countries>(group.Key.ToString());
            var tickers = group.Select(x => x).ToArray();
            if (country != Countries.Rus)
                tickers = tickers.Select(x => new CompanyDto($"{x.Id}-RM", group.Key, x.Name))
                    .ToArray();

            var idIndex = country == Countries.Rus ? 19 : 18;

            var response = await client.GetIsinsAsync(country);

            foreach (var (isin, ticker, name) in response.Securities.Data
                         .Select(x => (Ticker: x[0].ToString(), Isin: x[idIndex].ToString()))
                         .Join(tickers,
                             x => x.Ticker,
                             y => y.Id,
                             (x, y) => (x.Isin, x.Ticker, y.Name)))
            {
                if (isin!.Equals("0"))
                {
                    logger.LogWarning(nameof(GetRangeAsync), $"ISIN for {name} not recognized", isin);
                    continue;
                }

                var underlyingAsset = new UnderlyingAsset
                {
                    Id = isin,
                    UnderlyingAssetTypeId = (byte)UnderlyingAssetTypes.Stock,
                    CountryId = (byte)country,
                    Name = name
                };
                await underlyingAssetRepo.CreateUpdateAsync(new object[] { underlyingAsset.Id }, underlyingAsset, isin);

                var derivative = new Derivative
                {
                    Id = isin,
                    Code = country == Countries.Rus ? ticker! : ticker![..^3],
                    UnderlyingAssetId = isin
                };
                await derivativeRepo.CreateUpdateAsync(new object[] { derivative.Id, derivative.Code }, derivative, isin);
            }

            logger.LogDebug(nameof(GetRangeAsync), "ISINs set Ok", string.Join("; ", group.Select(x => x.Id)));
        }
    }
}