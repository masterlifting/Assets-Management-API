﻿using System;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Data.Services.DtoServices;

public class ReportsDtoManager
{
    private readonly Repository<Report> reportRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly string rabbitConnectionString;

    public ReportsDtoManager(
        IOptions<ServiceSettings> options,
        Repository<Report> reportRepository,
        Repository<DataAccess.Entities.Company> companyRepository)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        this.reportRepository = reportRepository;
        this.companyRepository = companyRepository;
    }

    public async Task<ResponseModel<ReportGetDto>> GetAsync(string companyId, int year, byte quarter)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var company = await companyRepository.FindAsync(companyId);

        if (company is null)
            return new() { Errors = new[] { $"'{companyId}' not found" } };

        var report = await reportRepository.FindAsync(company.Id, year, quarter);

        if (report is not null)
            return new()
            {
                Data = new()
                {
                    Ticker = company.Id,
                    Company = company.Name,
                    Year = report.Year,
                    Quarter = report.Quarter,
                    SourceType = report.SourceType,
                    Multiplier = report.Multiplier,
                    Asset = report.Asset,
                    CashFlow = report.CashFlow,
                    LongTermDebt = report.LongTermDebt,
                    Obligation = report.Obligation,
                    ProfitGross = report.ProfitGross,
                    ProfitNet = report.ProfitNet,
                    Revenue = report.Revenue,
                    ShareCapital = report.ShareCapital,
                    Turnover = report.Turnover
                }
            };

        return new()
        {
            Errors = new[] { $"Report for '{companyId}' not found" }
        };
    }
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetAsync(CompanyDataFilterByQuarter<Report> filter, HttpPagination pagination)
    {
        var filteredQuery = reportRepository.GetQuery(filter.FilterExpression);
        var count = await reportRepository.GetCountAsync(filteredQuery);
        var paginatedQuery = reportRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Year, x => x.Quarter);

        var result = await paginatedQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new ReportGetDto
            {
                Ticker = y.Id,
                Company = y.Name,
                Year = x.Year,
                Quarter = x.Quarter,
                SourceType = x.SourceType,
                Multiplier = x.Multiplier,
                Asset = x.Asset,
                CashFlow = x.CashFlow,
                LongTermDebt = x.LongTermDebt,
                Obligation = x.Obligation,
                ProfitGross = x.ProfitGross,
                ProfitNet = x.ProfitNet,
                Revenue = x.Revenue,
                ShareCapital = x.ShareCapital,
                Turnover = x.Turnover
            })
            .ToArrayAsync();

        return new()
        {
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLastAsync(CompanyDataFilterByQuarter<Report> filter, HttpPagination pagination)
    {
        var filteredQuery = reportRepository.GetQuery(filter.FilterExpression);

        var queryResult = await filteredQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new ReportGetDto
            {
                Ticker = y.Id,
                Company = y.Name,
                Year = x.Year,
                Quarter = x.Quarter,
                SourceType = x.SourceType,
                Multiplier = x.Multiplier,
                Asset = x.Asset,
                CashFlow = x.CashFlow,
                LongTermDebt = x.LongTermDebt,
                Obligation = x.Obligation,
                ProfitGross = x.ProfitGross,
                ProfitNet = x.ProfitNet,
                Revenue = x.Revenue,
                ShareCapital = x.ShareCapital,
                Turnover = x.Turnover
            })
            .ToArrayAsync();

        var groupedResult = queryResult
            .GroupBy(x => x.Company)
            .Select(x => x
                .OrderBy(y => y.Year)
                .ThenBy(y => y.Quarter)
                .Last())
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Quarter)
            .ThenBy(x => x.Company)
            .ToArray();

        return new()
        {
            Data = new()
            {
                Items = pagination.GetPaginatedResult(groupedResult),
                Count = groupedResult.Length
            }
        };
    }

    public async Task<ResponseModel<string>> CreateAsync(ReportPostDto model)
    {
        var entity = new Report
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            Year = model.Year,
            Quarter = model.Quarter,
            SourceType = model.SourceType,
            Multiplier = model.Multiplier,
            Asset = model.Asset,
            CashFlow = model.CashFlow,
            LongTermDebt = model.LongTermDebt,
            Obligation = model.Obligation,
            ProfitGross = model.ProfitGross,
            ProfitNet = model.ProfitNet,
            Revenue = model.Revenue,
            ShareCapital = model.ShareCapital,
            Turnover = model.Turnover
        };

        var message = $"Report of '{entity.CompanyId}' create at {entity.Year} - {entity.Quarter}";

        var (error, _) = await reportRepository.CreateAsync(entity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<ReportPostDto> models)
    {
        var dtos = models.ToArray();

        if (!dtos.Any())
            return new() { Errors = new[] { "Report data for creating not found" } };

        var entities = dtos.Select(x => new Report
        {
            CompanyId = string.Intern(x.CompanyId.Trim().ToUpperInvariant()),
            Year = x.Year,
            Quarter = x.Quarter,
            SourceType = x.SourceType,
            Multiplier = x.Multiplier,
            Asset = x.Asset,
            CashFlow = x.CashFlow,
            LongTermDebt = x.LongTermDebt,
            Obligation = x.Obligation,
            ProfitGross = x.ProfitGross,
            ProfitNet = x.ProfitNet,
            Revenue = x.Revenue,
            ShareCapital = x.ShareCapital,
            Turnover = x.Turnover
        })
        .ToArray();

        var (error, result) = await reportRepository.CreateAsync(entities, new CompanyQuarterComparer<Report>(), $"Source count: {entities.Length}");

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = $"Reports count: {result.Length} was successed" };
    }
    public async Task<ResponseModel<string>> UpdateAsync(ReportPostDto model)
    {
        var entity = new Report
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            Year = model.Year,
            Quarter = model.Quarter,
            SourceType = model.SourceType,
            Multiplier = model.Multiplier,
            Asset = model.Asset,
            CashFlow = model.CashFlow,
            LongTermDebt = model.LongTermDebt,
            Obligation = model.Obligation,
            ProfitGross = model.ProfitGross,
            ProfitNet = model.ProfitNet,
            Revenue = model.Revenue,
            ShareCapital = model.ShareCapital,
            Turnover = model.Turnover
        };

        var info = $"Report of '{model.CompanyId}' update at {entity.Year} - {entity.Quarter}";

        var (error, _) = await reportRepository.UpdateAsync(new object[] { entity.CompanyId, entity.Year, entity.Quarter }, entity, info );

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId, int year, byte quarter)
    {
        companyId = string.Intern(companyId.Trim().ToUpperInvariant());

        var info = $"Report of '{companyId}' delete at {year} - {quarter}";

        var (error, _) = await reportRepository.DeleteByIdAsync(new object[]{ companyId, year, quarter } , info );

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }

    public string Load()
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Reports, QueueActions.Call, DateTime.UtcNow.ToShortDateString());
        return "Load reports is running...";
    }
    public string Load(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Report, QueueActions.Call, companyId);
        return $"Load reports for '{companyId}' is running...";
    }
}