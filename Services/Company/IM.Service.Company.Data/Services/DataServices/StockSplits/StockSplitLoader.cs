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

namespace IM.Service.Company.Data.Services.DataServices.StockSplits;

public class StockSplitLoader : IDataLoad<StockSplit, DateDataConfigModel>
{
    private readonly ILogger<StockSplitLoader> logger;
    private readonly StockSplitParser parser;
    private readonly Repository<StockSplit> stockSplitRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<CompanySourceType> companySourceTypeRepository;
    public StockSplitLoader(
        ILogger<StockSplitLoader> logger,
        StockSplitParser parser,
        Repository<StockSplit> stockSplitRepository,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<CompanySourceType> companySourceTypeRepository)
    {
        this.logger = logger;
        this.parser = parser;
        this.stockSplitRepository = stockSplitRepository;
        this.companyRepository = companyRepository;
        this.companySourceTypeRepository = companySourceTypeRepository;
    }

    public async Task<StockSplit[]> DataSetAsync(string companyId)
    {
        var result = Array.Empty<StockSplit>();
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
            logger.LogWarning(LogEvents.Processing, "Sources for {company}' was not found", company.Name);
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

            var (error, savedResult) = await stockSplitRepository.CreateUpdateAsync(loadedData, new CompanyDateComparer<StockSplit>(), $"Stock splits for {company.Name}");

            if (error is null)
                result = result.Concat(savedResult).ToArray();
        }

        return result;
    }
    public async Task<StockSplit[]> DataSetAsync()
    {
        var lasts = await GetLastDatabaseDataAsync();
        var lastsDictionary = lasts.ToDictionary(x => x.CompanyId, y => y.Date);
        var companySourceTypes = await companyRepository.GetDbSet()
            .Join(companySourceTypeRepository.GetDbSet(), x => x.Id, y => y.CompanyId, (x, y) => new
            {
                CompanyId = x.Id,
                SourceName = y.SourceType.Name,
                SourceValue = y.Value
            })
            .ToArrayAsync();

        var result = Array.Empty<StockSplit>();

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

            var (error, savedResult) = await stockSplitRepository.CreateUpdateAsync(loadedData, new CompanyDateComparer<StockSplit>(), $"Stock splits for source: {source.Key}");

            if (error is null)
                result = result.Concat(savedResult).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "Stock splits count of companies: {count} was processed.", result.GroupBy(x => x.CompanyId).Count());
        return result;
    }

    public async Task<StockSplit?> GetLastDatabaseDataAsync(string companyId)
    {
        var stockSplits = await stockSplitRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Date >= DateTime.UtcNow.AddMonths(-1));
        return stockSplits
            .OrderBy(x => x.Date)
            .LastOrDefault();
    }
    public async Task<StockSplit[]> GetLastDatabaseDataAsync()
    {
        var stockSplits = await stockSplitRepository.GetSampleAsync(x => x.Date >= DateTime.UtcNow.AddMonths(-1));
        return stockSplits
            .GroupBy(x => x.CompanyId)
            .Select(x =>
                x.OrderBy(y => y.Date)
                    .Last())
            .ToArray();
    }

    public async Task<StockSplit[]> DataGetAsync(string source, DateDataConfigModel config) =>
        await parser.GetStockSplitsAsync(source, config);
    public async Task<StockSplit[]> DataGetAsync(string source, IEnumerable<DateDataConfigModel> config)
    {
        var _data = config.ToArray();

        return !_data.Any()
            ? Array.Empty<StockSplit>()
            : await parser.GetStockSplitsAsync(source, _data);
    }
}