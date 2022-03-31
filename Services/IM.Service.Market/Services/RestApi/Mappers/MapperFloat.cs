using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.RestApi.Mappers;

public class MapperFloat : IMapperRead<Float, FloatGetDto>, IMapperWrite<Float, FloatPostDto>
{
    public Task<FloatGetDto> MapFromAsync(Float entity) => Task.FromResult<FloatGetDto>(new ()
    {
        Company = entity.Company.Name,
        Source = entity.Source.Name,
        Date = entity.Date,

        Value = entity.Value,
        ValueFree = entity.ValueFree
    });
    public async Task<FloatGetDto[]> MapFromAsync(IQueryable<Float> query) => await query.Select(x => new FloatGetDto
    {
        Company = x.Company.Name,
        Source = x.Source.Name,
        Date = x.Date,

        Value = x.Value,
        ValueFree = x.ValueFree
    }).ToArrayAsync();
    public async Task<FloatGetDto[]> MapLastFromAsync(IQueryable<Float> query)
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

    public Float MapTo(FloatPostDto model) => new()
    {
        CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
        SourceId = model.SourceId,
        Date =  model.Date,

        Value = model.Value,
        ValueFree = model.ValueFree
    };
    public Float MapTo(Float id, FloatPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.Trim().ToUpperInvariant()),
        SourceId = id.SourceId,
        Date = id.Date,

        Value = model.Value,
        ValueFree = model.ValueFree
    };
    public Float[] MapTo(IEnumerable<FloatPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(MapTo).ToArray()
            : Array.Empty<Float>();
    }
}