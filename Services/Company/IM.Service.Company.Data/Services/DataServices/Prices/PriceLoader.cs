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

public class PriceLoader : IDataLoad<Price, DateDataConfigModel>
{
    private readonly ILogger<PriceLoader> logger;
    private readonly Repository<Price> priceRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<CompanySourceType> companySourceTypeRepository;
    private readonly PriceParser parser;
    public PriceLoader(
        ILogger<PriceLoader> logger,
        Repository<Price> priceRepository,
        PriceParser parser,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<CompanySourceType> companySourceTypeRepository)
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
            logger.LogWarning(LogEvents.Processing, "'Sources for {company}' was not found", company.Name);
            return result;
        }

        foreach (var source in sources)
        {
            if (!parser.IsSource(source.Name))
                continue;

            var last = await GetLastDatabaseDataAsync(company.Id);

            DateDataConfigModel config = last is not null
                ? new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Date = DateOnly.FromDateTime(last.Date)
                }
                : new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
                };

            var loadedData = await DataGetAsync(source.Name, config);

            if (!loadedData.Any())
                continue;

            var (error, savedResult) = await priceRepository.CreateUpdateAsync(loadedData, new CompanyDateComparer<Price>(), $"Prices for {company.Name}");

            if (error is null)
                result = result.Concat(savedResult).ToArray();
        }

        return result;
    }
    public async Task<Price[]> DataSetAsync()
    {
        var lasts = await GetLastDatabaseDataAsync();
        var lastsDictionary = lasts.ToDictionary(x => x.CompanyId, y => y.Date);
        var companySourceTypes = await companyRepository.GetDbSet()
            .Join(companySourceTypeRepository.GetDbSet(), 
x => x.Id, 
y => y.CompanyId, 
(x, y) => new
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

            var config = source
                .Select(x => lastsDictionary.ContainsKey(x.CompanyId)
                    ? new DateDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Date = DateOnly.FromDateTime(lastsDictionary[x.CompanyId].Date)
                    }
                    : new DateDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
                    })
                .ToArray();

            var loadedData = await DataGetAsync(source.Key, config);

            if (!loadedData.Any())
                continue;

            var (error, savedResult) = await priceRepository.CreateUpdateAsync(loadedData, new CompanyDateComparer<Price>(), $"Prices for source: {source.Key}");

            if (error is null)
                result = result.Concat(savedResult).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "Prices count of companies: {count} was processed.", result.GroupBy(x => x.CompanyId).Count());
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

    public async Task<Price[]> DataGetAsync(string source, DateDataConfigModel config)
    {
        var result = await parser.LoadLastPricesAsync(source, config);

        if (config.Date < GetExchangeWorkDate(source))
            result = result.Concat(await parser.LoadHistoryPricesAsync(source, config)).ToArray();

        return result;
    }
    public async Task<Price[]> DataGetAsync(string source, IEnumerable<DateDataConfigModel> config)
    {
        config = config.ToArray();

        if (!config.Any())
            return Array.Empty<Price>();

        var exchangeDate = GetExchangeWorkDate(source);

        var dataToHistory = config.Where(x => x.Date < exchangeDate).ToArray();

        var result = await parser.LoadLastPricesAsync(source, config);

        if (dataToHistory.Any())
            result = result.Concat(await parser.LoadHistoryPricesAsync(source, dataToHistory)).ToArray();

        return result;
    }
}