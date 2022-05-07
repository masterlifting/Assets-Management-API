using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.HttpRestApi.Mappers;

public class MapperReport : IMapperRead<Report, ReportGetDto>, IMapperWrite<Report, ReportPostDto>
{
    public async Task<ReportGetDto[]> MapFromAsync(IQueryable<Report> query) => await query
        .OrderByDescending(x => x.Year)
        .ThenByDescending(x => x.Quarter)
        .Select(x => new ReportGetDto
        {
            Company = x.Company.Name,
            Source = x.Source.Name,
            Year = x.Year,
            Quarter = x.Quarter,

            Multiplier = x.Multiplier,
            Currency = x.Currency.Name,

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
    public async Task<ReportGetDto[]> MapLastFromAsync(IQueryable<Report> query)
    {
        var queryResult = await query
            .Select(x => new ReportGetDto
            {
                Company = x.Company.Name,
                Source = x.Source.Name,
                Year = x.Year,
                Quarter = x.Quarter,

                Multiplier = x.Multiplier,
                Currency = x.Currency.Name,

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

        return queryResult
                .GroupBy(x => x.Company)
                .Select(x => x
                    .OrderBy(y => y.Year)
                    .ThenBy(y => y.Quarter)
                    .Last())
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .ThenBy(x => x.Company)
                .ToArray();
    }

    public Report MapTo(Report id, ReportPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.ToUpperInvariant()),
        SourceId = id.SourceId,
        Year = id.Year,
        Quarter = id.Quarter,

        Multiplier = model.Multiplier,
        CurrencyId = model.CurrencyId,

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
    public Report MapTo(string companyId, byte sourceId, ReportPostDto model) => new()
    {
        CompanyId = string.Intern(companyId),
        SourceId = sourceId,
        Year = model.Year,
        Quarter = model.Quarter,

        Multiplier = model.Multiplier,
        CurrencyId = model.CurrencyId,

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
    public Report[] MapTo(string companyId, byte sourceId, IEnumerable<ReportPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(x => MapTo(companyId, sourceId, x)).ToArray()
            : Array.Empty<Report>();
    }
}