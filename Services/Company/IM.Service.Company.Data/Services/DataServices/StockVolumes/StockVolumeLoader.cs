using IM.Service.Common.Net;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Comparators;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes;

public class StockVolumeLoader : IDataLoad<StockVolume, DateDataConfigModel>
{
    private readonly ILogger<StockVolumeLoader> logger;
    private readonly StockVolumeParser parser;
    private readonly Repository<StockVolume> stockVolumeRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<CompanySourceType> companySourceTypeRepository;
    public StockVolumeLoader(
        ILogger<StockVolumeLoader> logger,
        StockVolumeParser parser,
        Repository<StockVolume> stockVolumeRepository,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<CompanySourceType> companySourceTypeRepository)
    {
        this.logger = logger;
        this.parser = parser;
        this.stockVolumeRepository = stockVolumeRepository;
        this.companyRepository = companyRepository;
        this.companySourceTypeRepository = companySourceTypeRepository;
    }

    public async Task<StockVolume[]> DataSetAsync(string companyId)
    {
        var result = Array.Empty<StockVolume>();
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
                    Date = last.Date
                }
                : new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Date = DateTime.UtcNow.AddYears(-1)
                };

            var loadedData = await DataGetAsync(source.Name, config);

            if (!loadedData.Any())
                continue;

            var (error, savedResult) = await stockVolumeRepository.CreateUpdateAsync(loadedData, new StockVolumeComparer(), $"Stock volumes for {company.Name}");

            if (error is null)
                result = result.Concat(savedResult!).ToArray();
        }

        return result;
    }
    public async Task<StockVolume[]> DataSetAsync()
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

        var result = Array.Empty<StockVolume>();

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
                        Date = lastsDictionary[x.CompanyId].Date
                    }
                    : new DateDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Date = DateTime.UtcNow.AddYears(-1)
                    })
                .ToArray();

            var loadedData = await DataGetAsync(source.Key, config);

            if (!loadedData.Any())
                continue;

            var (error, savedResult) = await stockVolumeRepository.CreateUpdateAsync(loadedData, new StockVolumeComparer(), $"Stock volumes for source: {source.Key}");

            if (error is null)
                result = result.Concat(savedResult!).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "Stock volumes count of companies: {count} was processed.", result.GroupBy(x => x.CompanyId).Count());
        return result;
    }

    public async Task<StockVolume?> GetLastDatabaseDataAsync(string companyId)
    {
        var stockVolumes = await stockVolumeRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Date >= DateTime.UtcNow.AddMonths(-1));
        return stockVolumes
            .OrderBy(x => x.Date)
            .LastOrDefault();
    }
    public async Task<StockVolume[]> GetLastDatabaseDataAsync()
    {
        var stockVolumes = await stockVolumeRepository.GetSampleAsync(x => x.Date >= DateTime.UtcNow.AddMonths(-1));
        return stockVolumes
            .GroupBy(x => x.CompanyId)
            .Select(x =>
                x.OrderBy(y => y.Date)
                    .Last())
            .ToArray();
    }

    public async Task<StockVolume[]> DataGetAsync(string source, DateDataConfigModel config) =>
        await parser.GetStockVolumesAsync(source, config);
    public async Task<StockVolume[]> DataGetAsync(string source, IEnumerable<DateDataConfigModel> config)
    {
        var _data = config.ToArray();

        return !_data.Any()
            ? Array.Empty<StockVolume>()
            : await parser.GetStockVolumesAsync(source, _data);
    }
}