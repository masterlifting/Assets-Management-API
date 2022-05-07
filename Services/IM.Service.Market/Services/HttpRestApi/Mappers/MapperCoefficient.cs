using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Market.Services.HttpRestApi.Mappers.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.HttpRestApi.Mappers;

public class MapperCoefficient : IMapperRead<Coefficient, CoefficientGetDto>, IMapperWrite<Coefficient, CoefficientPostDto>
{
    public async Task<CoefficientGetDto[]> MapFromAsync(IQueryable<Coefficient> query) => await query
        .OrderByDescending(x => x.Year)
        .ThenByDescending(x => x.Quarter)
        .Select(x => new CoefficientGetDto
        {
            Company = x.Company.Name,
            Source = x.Source.Name,
            Year = x.Year,
            Quarter = x.Quarter,

            Pe = x.Pe,
            Pb = x.Pb,
            Profitability = x.Profitability,
            Roa = x.Roa,
            Roe = x.Roe,
            Eps = x.Eps,
            DebtLoad = x.DebtLoad
        })
        .ToArrayAsync();
    public async Task<CoefficientGetDto[]> MapLastFromAsync(IQueryable<Coefficient> query)
    {
        var queryResult = await query
            .Select(x => new CoefficientGetDto
            {
                Company = x.Company.Name,
                Source = x.Source.Name,
                Year = x.Year,
                Quarter = x.Quarter,

                Pe = x.Pe,
                Pb = x.Pb,
                Profitability = x.Profitability,
                Roa = x.Roa,
                Roe = x.Roe,
                Eps = x.Eps,
                DebtLoad = x.DebtLoad
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

    public Coefficient MapTo(Coefficient id, CoefficientPostDto model) => new()
    {
        CompanyId = string.Intern(id.CompanyId.ToUpperInvariant()),
        SourceId = id.SourceId,
        Year = id.Year,
        Quarter = id.Quarter,

        Pe = model.Pe,
        Pb = model.Pb,
        Profitability = model.Profitability,
        Roa = model.Roa,
        Roe = model.Roe,
        Eps = model.Eps,
        DebtLoad = model.DebtLoad
    };
    public Coefficient MapTo(string companyId, byte sourceId, CoefficientPostDto model) => new()
    {
        CompanyId = string.Intern(companyId),
        SourceId = sourceId,
        Year = model.Year,
        Quarter = model.Quarter,

        Pe = model.Pe,
        Pb = model.Pb,
        Profitability = model.Profitability,
        Roa = model.Roa,
        Roe = model.Roe,
        Eps = model.Eps,
        DebtLoad = model.DebtLoad
    };
    public Coefficient[] MapTo(string companyId, byte sourceId, IEnumerable<CoefficientPostDto> models)
    {
        var dtos = models.ToArray();
        return dtos.Any()
            ? dtos.Select(x => MapTo(companyId, sourceId, x)).ToArray()
            : Array.Empty<Coefficient>();
    }
}