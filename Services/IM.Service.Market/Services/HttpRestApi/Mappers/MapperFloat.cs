using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.HttpRestApi.Mappers;

public class MapperFloat : IMapperRead<Float, FloatGetDto>, IMapperWrite<Float, FloatPostDto>
{
    public async Task<FloatGetDto[]> MapFromAsync(IQueryable<Float> query) => await query
        .OrderByDescending(x => x.Date)
        .Select(x => new FloatGetDto
        {
            Company = x.Company.Name,
            Source = x.Source.Name,
            Date = x.Date,

            Value = x.Value,
            ValueFree = x.ValueFree
        })
        .ToArrayAsync();
    public async Task<FloatGetDto[]> MapLastFromAsync(IQueryable<Float> query)
    {
        var queryResult = await query
            .Select(x => new FloatGetDto
            {
                Company = x.Company.Name,
                Source = x.Source.Name,
                Date = x.Date,

                Value = x.Value,
                ValueFree = x.ValueFree
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

    public Float MapTo(Float id, FloatPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.ToUpperInvariant()),
        SourceId = id.SourceId,
        Date = id.Date,

        Value = model.Value,
        ValueFree = model.ValueFree
    };
    public Float MapTo(string companyId, byte sourceId, FloatPostDto model) => new()
    {
        CompanyId = string.Intern(companyId),
        SourceId = sourceId,
        Date = model.Date,

        Value = model.Value,
        ValueFree = model.ValueFree
    };
    public Float[] MapTo(string companyId, byte sourceId, IEnumerable<FloatPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(x => MapTo(companyId, sourceId, x)).ToArray()
            : Array.Empty<Float>();
    }
}