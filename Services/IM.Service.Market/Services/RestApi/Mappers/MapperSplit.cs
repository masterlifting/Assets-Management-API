using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.RestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.RestApi.Mappers;

public class MapperSplit : IMapperRead<Split, SplitGetDto>, IMapperWrite<Split, SplitPostDto>
{
    public Task<SplitGetDto> MapFromAsync(Split entity) => Task.FromResult<SplitGetDto>(new ()
    {
        Company = entity.Company.Name,
        Source = entity.Source.Name,
        Date = entity.Date,

        Value = entity.Value
    });
    public async Task<SplitGetDto[]> MapFromAsync(IQueryable<Split> query) => await query.Select(x => new SplitGetDto
    {
        Company = x.Company.Name,
        Source = x.Source.Name,
        Date = x.Date,

        Value = x.Value
    }).ToArrayAsync();
    public async Task<SplitGetDto[]> MapLastFromAsync(IQueryable<Split> query)
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

    public Split MapTo(SplitPostDto model) => new()
    {
        CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
        SourceId = model.SourceId,
        Date = model.Date,

        Value = model.Value
    };
    public Split MapTo(Split id, SplitPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.Trim().ToUpperInvariant()),
        SourceId = id.SourceId,
        Date = id.Date,

        Value = model.Value
    };
    public Split[] MapTo(IEnumerable<SplitPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(MapTo).ToArray()
            : Array.Empty<Split>();
    }
}