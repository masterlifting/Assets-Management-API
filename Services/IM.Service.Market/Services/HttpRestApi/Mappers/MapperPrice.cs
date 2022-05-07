using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.HttpRestApi.Mappers;

public class MapperPrice : IMapperRead<Price, PriceGetDto>, IMapperWrite<Price, PricePostDto>
{
    public async Task<PriceGetDto[]> MapFromAsync(IQueryable<Price> query) => await query
        .OrderByDescending(x => x.Date)
        .Select(x => new PriceGetDto
        {
            Company = x.Company.Name,
            Source = x.Source.Name,
            Date = x.Date,

            Currency = x.Currency.Name,
            Value = x.Value,
            ValueTrue = x.ValueTrue
        })
        .ToArrayAsync();
    public async Task<PriceGetDto[]> MapLastFromAsync(IQueryable<Price> query)
    {
        var queryResult = await query
            .Select(x => new PriceGetDto
            {
                Company = x.Company.Name,
                Source = x.Source.Name,
                Date = x.Date,

                Currency = x.Currency.Name,
                Value = x.Value,
                ValueTrue = x.ValueTrue
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

    public Price MapTo(Price id, PricePostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.ToUpperInvariant()),
        SourceId = id.SourceId,
        Date = id.Date,

        CurrencyId = model.CurrencyId,
        Value = model.Value
    };
    public Price MapTo(string companyId, byte sourceId, PricePostDto model) => new()
    {
        CompanyId = string.Intern(companyId),
        SourceId = sourceId,
        Date = model.Date,

        CurrencyId = model.CurrencyId,
        Value = model.Value
    };
    public Price[] MapTo(string companyId, byte sourceId, IEnumerable<PricePostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(x => MapTo(companyId, sourceId, x)).ToArray()
            : Array.Empty<Price>();
    }
}