using IM.Service.Common.Net;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes;

public class StockVolumeLoader : IDataLoader<StockVolume>
{
    private readonly ILogger<StockVolumeLoader> logger;
    private readonly StockVolumeGrabber grabber;
    private readonly Repository<StockVolume> stockVolumeRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<CompanySource> companySourceRepository;
    public StockVolumeLoader(
        ILogger<StockVolumeLoader> logger,
        StockVolumeGrabber grabber,
        Repository<StockVolume> stockVolumeRepository,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<CompanySource> companySourceRepository)
    {
        this.logger = logger;
        this.grabber = grabber;
        this.stockVolumeRepository = stockVolumeRepository;
        this.companyRepository = companyRepository;
        this.companySourceRepository = companySourceRepository;
    }

    public async Task<StockVolume?> GetLastDataAsync(string companyId)
    {
        var stockVolumes = await stockVolumeRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Date >= DateTime.UtcNow.AddMonths(-1));
        return stockVolumes
            .OrderBy(x => x.Date)
            .LastOrDefault();
    }
    public async Task<StockVolume[]> GetLastDataAsync()
    {
        var stockVolumes = await stockVolumeRepository.GetSampleAsync(x => x.Date >= DateTime.UtcNow.AddMonths(-1));
        return stockVolumes
            .GroupBy(x => x.CompanyId)
            .Select(x =>
                x.OrderBy(y => y.Date)
                    .Last())
            .ToArray();
    }

    public async Task DataSetAsync(string companyId)
    {
        var company = await companyRepository.FindAsync(companyId);

        if (company is null)
        {
            logger.LogWarning(LogEvents.Processing, "'{companyId}' not found", companyId);
            return;
        }

        var sources = await companySourceRepository
            .GetSampleAsync(x => x.CompanyId == company.Id, x => new
            {
                x.Source.Name,
                x.Value
            });

        if (!sources.Any())
        {
            logger.LogWarning(LogEvents.Processing, "sources of '{company}' not found", company.Name);
            return;
        }

        foreach (var source in sources)
        {
            if (!grabber.IsSource(source.Name))
                continue;

            var last = await GetLastDataAsync(company.Id);

            DataConfigModel config = new()
            {
                CompanyId = company.Id,
                SourceValue = source.Value,
                IsCurrent = last is not null && last.Date.Date == DateTime.UtcNow.Date
            };

            if (config.IsCurrent)
                await grabber.GrabCurrentDataAsync(source.Name, config);
            else
                await grabber.GrabHistoryDataAsync(source.Name, config);
        }
    }
    public async Task DataSetAsync()
    {
        var lasts = await GetLastDataAsync();
        var lastsDictionary = lasts.ToDictionary(x => x.CompanyId, y => y.Date);
        var companySourceTypes = await companyRepository.GetDbSet()
            .Join(companySourceRepository.GetDbSet(), x => x.Id, y => y.CompanyId, (x, y) => new
            {
                CompanyId = x.Id,
                SourceName = y.Source.Name,
                SourceValue = y.Value
            })
            .ToArrayAsync();

        foreach (var source in companySourceTypes.GroupBy(x => x.SourceName))
        {
            if (!grabber.IsSource(source.Key))
                continue;

            var configs = source
                .Select(x =>
                    new DataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        IsCurrent = lastsDictionary.ContainsKey(x.CompanyId) &&
                                    lastsDictionary[x.CompanyId].Date == DateTime.UtcNow.Date
                    })
                .ToArray();

            await grabber.GrabHistoryDataAsync(source.Key, configs.Where(x => !x.IsCurrent));
        }
    }
}