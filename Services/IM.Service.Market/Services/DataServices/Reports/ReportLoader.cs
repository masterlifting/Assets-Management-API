using IM.Service.Common.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Entities.ManyToMany;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Models.Data;
using static  IM.Service.Market.Services.DataServices.Reports.ReportHelper;

namespace IM.Service.Market.Services.DataServices.Reports;

public class ReportLoader : IDataLoader<Report>
{
    private readonly ILogger<ReportLoader> logger;
    private readonly ReportGrabber grabber;
    private readonly Repository<Report> reportRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<CompanySource> companySourceRepository;
    public ReportLoader(
        ILogger<ReportLoader> logger,
        ReportGrabber grabber,
        Repository<Report> reportRepository,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<CompanySource> companySourceRepository)
    {
        this.logger = logger;
        this.grabber = grabber;
        this.reportRepository = reportRepository;
        this.companyRepository = companyRepository;
        this.companySourceRepository = companySourceRepository;
    }

    public async Task<Report?> GetLastDataAsync(string companyId)
    {
        var reports = await reportRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Year >= DateTime.UtcNow.AddYears(-3).Year);
        return reports
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Quarter)
            .LastOrDefault();
    }
    public async Task<Report[]> GetLastDataAsync()
    {
        var reports = await reportRepository.GetSampleAsync(x => x.Year >= DateTime.UtcNow.AddYears(-3).Year);
        return reports
            .GroupBy(x => x.CompanyId)
            .Select(x =>
                x.OrderBy(y => y.Year)
                    .ThenBy(y => y.Quarter)
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
                IsCurrent = last is not null && IsMissingLastQuarter(last.Year,last.Quarter)
            };

            await grabber.GrabHistoryDataAsync(source.Name, config);
        }
    }
    public async Task DataSetAsync()
    {
        var lasts = await GetLastDataAsync();
        var lastsDictionary = lasts.ToDictionary(x => x.CompanyId, y => (y.Year, y.Quarter));
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
                        IsCurrent =
                            lastsDictionary.ContainsKey(x.CompanyId) 
                            && IsMissingLastQuarter(lastsDictionary[x.CompanyId].Year, lastsDictionary[x.CompanyId].Quarter)
                    })
                .ToArray();

            await grabber.GrabHistoryDataAsync(source.Key, configs.Where(x => !x.IsCurrent));
        }

        logger.LogInformation(LogEvents.Processing, "Place: {place}. Is complete.", nameof(DataSetAsync));
    }
}