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

using static IM.Service.Company.Data.Services.DataServices.Reports.ReportHelper;

namespace IM.Service.Company.Data.Services.DataServices.Reports;

public class ReportLoader : IDataLoad<Report, ReportDataConfigModel>
{
    private readonly ILogger<ReportLoader> logger;
    private readonly ReportParser parser;
    private readonly RepositorySet<Report> reportRepository;
    private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
    private readonly RepositorySet<CompanySourceType> companySourceTypeRepository;
    public ReportLoader(
        ILogger<ReportLoader> logger,
        ReportParser parser,
        RepositorySet<Report> reportRepository,
        RepositorySet<DataAccess.Entities.Company> companyRepository,
        RepositorySet<CompanySourceType> companySourceTypeRepository)
    {
        this.logger = logger;
        this.parser = parser;
        this.reportRepository = reportRepository;
        this.companyRepository = companyRepository;
        this.companySourceTypeRepository = companySourceTypeRepository;
    }

    public async Task<Report[]> DataSetAsync(string companyId)
    {
        var result = Array.Empty<Report>();
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

            var lastReport = await GetLastDatabaseDataAsync(company.Id);

            ReportDataConfigModel config = lastReport is null
                ? new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Year = DateTime.UtcNow.AddYears(-3).Year,
                    Quarter = 1
                }
                : new()
                {
                    CompanyId = company.Id,
                    SourceValue = source.Value,
                    Year = lastReport.Year,
                    Quarter = lastReport.Quarter
                };

            var reports = await DataGetAsync(source.Name, config);

            result = !result.Any()
                ? await SaveAsync(reports)
                : result.Concat(await SaveAsync(reports)).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "reports count: {count} for '{company}' was loaded", result.Length, company.Name);
        return result;
    }
    public async Task<Report[]> DataSetAsync()
    {
        var lastReports = await GetLastDatabaseDataAsync();
        var lastReportsDictionary = lastReports.ToDictionary(x => x.CompanyId, y => (y.Year, y.Quarter));
        var companySourceTypes = await companyRepository.GetDbSet()
            .Join(companySourceTypeRepository.GetDbSet(), x => x.Id, y => y.CompanyId, (x, y) => new
            {
                CompanyId = x.Id,
                SourceName = y.SourceType.Name,
                SourceValue = y.Value
            })
            .ToArrayAsync();

        var result = Array.Empty<Report>();

        foreach (var source in companySourceTypes.GroupBy(x => x.SourceName))
        {
            if (!parser.IsSource(source.Key))
                continue;

            var config = source
                .Select(x => lastReportsDictionary.ContainsKey(x.CompanyId)
                    ? new ReportDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Year = lastReportsDictionary[x.CompanyId].Year,
                        Quarter = lastReportsDictionary[x.CompanyId].Quarter
                    }
                    : new ReportDataConfigModel
                    {
                        CompanyId = x.CompanyId,
                        SourceValue = x.SourceValue,
                        Year = DateTime.UtcNow.AddYears(-3).Year,
                        Quarter = 1
                    })
                .ToArray();

            var reports = await DataGetAsync(source.Key, config);

            result = !result.Any()
                ? await SaveAsync(reports)
                : result.Concat(await SaveAsync(reports)).ToArray();
        }

        if (result.Length <= 0)
            return result;

        logger.LogInformation(LogEvents.Processing, "reports count of companies: {count} was processed.", result.GroupBy(x => x.CompanyId).Count());
        return result;
    }

    public async Task<Report?> GetLastDatabaseDataAsync(string companyId)
    {
        var reports = await reportRepository.GetSampleAsync(x => x.CompanyId == companyId && x.Year >= DateTime.UtcNow.AddYears(-3).Year);
        return reports
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Quarter)
            .LastOrDefault();
    }
    public async Task<Report[]> GetLastDatabaseDataAsync()
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

    public async Task<Report[]> DataGetAsync(string source, ReportDataConfigModel config) =>
        IsMissingLastQuarter(logger, config)
            ? await parser.GetReportsAsync(source, config)
            : Array.Empty<Report>();
    public async Task<Report[]> DataGetAsync(string source, IEnumerable<ReportDataConfigModel> config)
    {
        var _data = config.ToArray();

        return !_data.Any()
            ? Array.Empty<Report>()
            : await parser.GetReportsAsync(source, _data.Where(x => IsMissingLastQuarter(logger, x)));
    }

    public async Task<Report[]> SaveAsync(IEnumerable<Report> entities)
    {
        var (error, result) = await reportRepository.CreateUpdateAsync(entities, new CompanyQuarterComparer<Report>(), "entities");
        return error is not null ? Array.Empty<Report>() : result!;
    }
}