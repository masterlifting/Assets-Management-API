using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Models.Api.Http;
using IM.Service.MarketData.Services.RestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.MarketData.Services.RestApi.Mappers;

public class MapperPrice : IMapperRead<Price, PriceGetDto>, IMapperWrite<Price, PricePostDto>
{
    public Task<PriceGetDto> MapFromAsync(Price entity) => Task.FromResult<PriceGetDto>(new()
    {
        Company = entity.Company.Name,
        Source = entity.Source.Name,
        Date = entity.Date,

        Value = entity.Value,
        ValueTrue = entity.ValueTrue
    });
    public async Task<PriceGetDto[]> MapFromAsync(IQueryable<Price> query) => await query.Select(x => new PriceGetDto
    {
        Company = x.Company.Name,
        Source = x.Source.Name,
        Date = x.Date,

        Value = x.Value,
        ValueTrue = x.ValueTrue
    }).ToArrayAsync();
    public async Task<PriceGetDto[]> MapLastFromAsync(IQueryable<Price> query)
    {
        var queryResult = await MapFromAsync(query);

        return queryResult
            .GroupBy(x => x.Company)
            .Select(x => x
                .OrderBy(y => y.Date)
                .Last())
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Company)
            .ToArray();
    }

    public Price MapTo(PricePostDto model) => new()
    {
        CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
        SourceId = model.SourceId,
        Date = model.Date,

        Value = model.Value
    };
    public Price MapTo(Price id, PricePostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.Trim().ToUpperInvariant()),
        SourceId = id.SourceId,
        Date = id.Date,

        Value = model.Value
    };
    public Price[] MapTo(IEnumerable<PricePostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(MapTo).ToArray()
            : Array.Empty<Price>();
    }
}