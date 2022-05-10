using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.Http.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.Http.Mappers;

public class MapperDividend : IMapperRead<Dividend, DividendGetDto>, IMapperWrite<Dividend, DividendPostDto>
{
    public async Task<DividendGetDto[]> MapFromAsync(IQueryable<Dividend> query) => await query
        .OrderByDescending(x => x.Date)
        .Select(x => new DividendGetDto
        {
            Company = x.Company.Name,
            Source = x.Source.Name,
            Date = x.Date,
            Currency = x.Currency.Name,

            Value = x.Value
        })
        .ToArrayAsync();
    public async Task<DividendGetDto[]> MapLastFromAsync(IQueryable<Dividend> query)
    {
        var queryResult = await query
            .Select(x => new DividendGetDto
            {
                Company = x.Company.Name,
                Source = x.Source.Name,
                Date = x.Date,
                Currency = x.Currency.Name,

                Value = x.Value
            })
            .ToArrayAsync();

        return queryResult
            .GroupBy(x => x.Company)
            .Select(x => x
                .OrderBy(y => y.Date)
                .Last())
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Company)
            .ToArray();
    }

    public Dividend MapTo(Dividend id, DividendPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.ToUpperInvariant()),
        SourceId = id.SourceId,
        Date = id.Date,

        Value = model.Value,
        CurrencyId = model.CurrencyId
    };
    public Dividend MapTo(string companyId, byte sourceId, DividendPostDto model) => new()
    {
        CompanyId = string.Intern(companyId),
        SourceId = sourceId,
        Date = model.Date,

        Value = model.Value,
        CurrencyId = model.CurrencyId
    };
    public Dividend[] MapTo(string companyId, byte sourceId, IEnumerable<DividendPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(x => MapTo(companyId, sourceId, x)).ToArray()
            : Array.Empty<Dividend>();
    }
}