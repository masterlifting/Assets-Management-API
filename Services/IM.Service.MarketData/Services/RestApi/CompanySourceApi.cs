using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.DataAccess.Comparators;
using IM.Service.MarketData.Domain.Entities.ManyToMany;
using IM.Service.MarketData.Models.Api.Http;

namespace IM.Service.MarketData.Services.RestApi;

public class CompanySourceApi
{
    private readonly Repository<CompanySource> companySourceRepo;
    public CompanySourceApi(Repository<CompanySource> companySourceRepo) => this.companySourceRepo = companySourceRepo;

    public async Task<ResponseModel<PaginatedModel<SourceGetDto>>> GetAsync(string companyId)
    {
        var sources = await companySourceRepo.GetSampleAsync(
            x => x.CompanyId == companyId,
            x => new SourceGetDto(x.Source.Name, x.Value));

        return new()
        {
            Data = new()
            {
                Count = sources.Length,
                Items = sources
            }
        };
    }
    public async Task<ResponseModel<SourceGetDto>> GetAsync(string companyId, byte sourceId)
    {
        var source = await companySourceRepo.FindAsync(companyId, sourceId);

        return source is not null
            ? new() { Data = new SourceGetDto(source.Source.Name, source.Value) }
            : new() { Errors = new[] { "Company source not found" } };
    }
    public async Task<(string?, CompanySource[])> CreateUpdateDeleteAsync(string companyId, IEnumerable<SourcePostDto> models) =>
        await companySourceRepo.CreateUpdateDeleteAsync(
            models
                .Select(x => new CompanySource
                {
                    CompanyId = companyId,
                    SourceId = x.Id,
                    Value = x.Value
                }), 
            new CompanySourceComparer(),
            "");
}