using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Api.Http;

namespace IM.Service.Market.Services.RestApi;

public class CompanySourceRestApi
{
    private readonly Repository<CompanySource> companySourceRepo;
    public CompanySourceRestApi(Repository<CompanySource> companySourceRepo) => this.companySourceRepo = companySourceRepo;

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