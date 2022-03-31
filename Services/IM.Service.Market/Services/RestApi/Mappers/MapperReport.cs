using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.RestApi.Mappers;

public class MapperReport : IMapperRead<Report, ReportGetDto>, IMapperWrite<Report, ReportPostDto>
{
    public Task<ReportGetDto> MapFromAsync(Report entity) => Task.FromResult<ReportGetDto>(new ()
    {
        Company = entity.Company.Name,
        Year = entity.Year,
        Quarter = entity.Quarter,
        Source = entity.Source.Name,
        Multiplier = entity.Multiplier,
        Asset = entity.Asset,
        CashFlow = entity.CashFlow,
        LongTermDebt = entity.LongTermDebt,
        Obligation = entity.Obligation,
        ProfitGross = entity.ProfitGross,
        ProfitNet = entity.ProfitNet,
        Revenue = entity.Revenue,
        ShareCapital = entity.ShareCapital,
        Turnover = entity.Turnover
    });
    public async Task<ReportGetDto[]> MapFromAsync(IQueryable<Report> query) => await query.Select(x => new ReportGetDto
    {
        Company = x.Company.Name,
        Source = x.Source.Name,
        Year = x.Year,
        Quarter = x.Quarter,
        
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
    }).ToArrayAsync();
    public async Task<ReportGetDto[]> MapLastFromAsync(IQueryable<Report> query)
    {
        var queryResult = await MapFromAsync(query);

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

    public Report MapTo(ReportPostDto model) => new()
    {
        CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
        SourceId = model.SourceId,
        Year = model.Year,
        Quarter = model.Quarter,

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
    public Report MapTo(Report id, ReportPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.Trim().ToUpperInvariant()),
        SourceId = id.SourceId,
        Year = id.Year,
        Quarter = id.Quarter,

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
    public Report[] MapTo(IEnumerable<ReportPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(MapTo).ToArray()
            : Array.Empty<Report>();
    }
}