using IM.Service.Common.Net;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Company.Data.Services.DataServices.Prices.PriceHelper;

namespace IM.Service.Company.Data.Services.DataServices.Prices;

public class PriceLoader : IDataLoad<Price, PriceDataConfigModel>
{
    private readonly ILogger<PriceLoader> logger;
    private readonly RepositorySet<Price> priceRepository;
    private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
    private readonly RepositorySet<CompanySourceType> companySourceTypeRepository;
    private readonly PriceParser parser;
    public PriceLoader(
        ILogger<PriceLoader> logger,
        RepositorySet<Price> priceRepository,
        PriceParser parser,
        RepositorySet<DataAccess.Entities.Company> companyRepository,
        RepositorySet<CompanySourceType> companySourceTypeRepository)
    {
        this.logger = logger;
        this.priceRepository = priceRepository;
        this.companyRepository = companyRepository;
        this.companySourceTypeRepository = companySourceTypeRepository;
        this.parser = parser;
    }

    public async Task<Price[]> DataSetAsync(string companyId)
    {
        var result = Array.Empty<Price>();
        var company = await companyRepository.FindAsync(companyId);

        if (company is null)
        {
            logger.LogWarning(LogEvents.Processing, "'{companyId}' was not found", companyId);
            return result;
        }

        var sources = await companySourceTypeRepository
            .GetSampleAsync(x => x.CompanyId == company.Id, x => new
            {
                x.SourceType.Name,
                x.Value
            });

        if (!sources.Any())
        {
            logger.LogWarning(LogEvents.Processing, "'{company}' sources was not found", company.Name);
            return result;
        }

        foreach (var source in sources)
        {
            if (!parser.IsSource(source.Name))
                continue;

            var isWeekend = IsExchangeWeekend(source.Name, DateTime.UtcNow);

            if (isWeekend)
            {
                logger.LogWarning(LogEvents.Processing, "'{source}' source has a day off", source.Name);
                continue;
            }

            var lastPrice = await GetLastDatabaseDataAsync(company.Id);

            PriceDataConfigModel config = lastPrice is not null
                ? new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Date = lastPrice.Date
                }
                : new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Date = DateTime.UtcNow.AddYears(-1)
                };

            var prices = await DataGetAsync(source.Name, config);

            result = !result.Any()
                ? await SaveAsync(prices)
                : result.Concat(await SaveAsync(prices)).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "prices count: {count} for '{company}' was loaded", result.Length, company.Name);
        return result;
    }
    public async Task<Price[]> DataSetAsync()
    {
        var lastPrices = await GetLastDatabaseDataAsync();
        var lastPricesDictionary = lastPrices.ToDictionary(x => x.CompanyId, y => y.Date);
        var companySourceTypes = await companyRepository.GetDbSet()
            .Join(companySourceTypeRepository.GetDbSet(), x => x.Id, y => y.CompanyId, (x, y) => new
            {
                CompanyId = x.Id,
                SourceName = y.SourceType.Name,
                SourceValue = y.Value
            })
            .ToArrayAsync();

        var result = Array.Empty<Price>();

        foreach (var source in companySourceTypes.GroupBy(x => x.SourceName))
        {
            if (!parser.IsSource(source.Key))
                continue;

            var isWeekend = IsExchangeWeekend(source.Key, DateTime.UtcNow);

            if (isWeekend)
            {
                logger.LogWarning(LogEvents.Processing, "'{source}' source has a day off", source.Key);
                continue;
            }

            var config = source
                .Select(x => lastPricesDictionary.ContainsKey(x.CompanyId)
                    ? new PriceDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Date = lastPricesDictionary[x.CompanyId].Date
                    }
                    : new PriceDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Date = DateTime.UtcNow.AddYears(-1)
                    })
                .ToArray();

            var prices = await DataGetAsync(source.Key, config);

            result = !result.Any()
                ? await SaveAsync(prices)
                : result.Concat(await SaveAsync(prices)).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "prices count of companies: {count} was processed.", result.GroupBy(x => x.CompanyId).Count());
        return result;
    }

    public async Task<Price?> GetLastDatabaseDataAsync(string companyId)
    {
        var prices = await priceRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Date >= DateTime.UtcNow.AddMonths(-1));
        return prices
            .OrderBy(x => x.Date)
            .LastOrDefault();
    }
    public async Task<Price[]> GetLastDatabaseDataAsync()
    {
        var prices = await priceRepository.GetSampleAsync(x => x.Date >= DateTime.UtcNow.AddMonths(-1));
        return prices
            .GroupBy(x => x.CompanyId)
            .Select(x =>
                x.OrderBy(y => y.Date)
                    .Last())
            .ToArray();
    }

    public async Task<Price[]> DataGetAsync(string source, PriceDataConfigModel config)
    {
        var result = await parser.LoadLastPricesAsync(source, config);

        if (config.Date < GetExchangeWorkDate(source))
            result = result.Concat(await parser.LoadHistoryPricesAsync(source, config)).ToArray();

        return result;
    }
    public async Task<Price[]> DataGetAsync(string source, IEnumerable<PriceDataConfigModel> config)
    {
        var _data = config.ToArray();

        if (!_data.Any())
            return Array.Empty<Price>();

        var exchangeDate = GetExchangeWorkDate(source);

        var dataToHistory = _data.Where(x => x.Date < exchangeDate).ToArray();

        var result = await parser.LoadLastPricesAsync(source, _data);

        if (dataToHistory.Any())
            result = result.Concat(await parser.LoadHistoryPricesAsync(source, dataToHistory)).ToArray();

        return result;
    }

    public async Task<Price[]> SaveAsync(IEnumerable<Price> entities)
    {
        var data = entities.ToArray();
        var result = new List<Price>(data.Length);

        var currentPrices = data.Where(x => x.Date == DateTime.UtcNow.Date);
        var restPrices = data.Where(x => x.Date != DateTime.UtcNow.Date);

        var (createError, createdResult) = await priceRepository.CreateAsync(restPrices, new CompanyDateComparer<Price>(), "current prices");
        var (updateError, updatedResult) = await priceRepository.CreateUpdateAsync(currentPrices, new CompanyDateComparer<Price>(), "rest prices");

        if (createError is not null)
            result.AddRange(createdResult!);

        if (updateError is not null)
            result.AddRange(updatedResult!);


        return result.ToArray();
    }
}