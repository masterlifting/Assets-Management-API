using IM.Service.Common.Net.Models.Services;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Api.Http;

namespace IM.Service.Market.Services.RestApi;

public class CompanySourceRestApi
{
    private readonly Repository<CompanySource> companySourceRepo;
    public CompanySourceRestApi(Repository<CompanySource> companySourceRepo) => this.companySourceRepo = companySourceRepo;

    public async Task<PaginationModel<SourceGetDto>> GetAsync(string companyId)
    {
        companyId = companyId.ToUpperInvariant();
        var sources = await companySourceRepo.GetSampleAsync(
            x => x.CompanyId == companyId,
            x => new SourceGetDto(x.SourceId, x.Source.Name, x.Value));

        return new PaginationModel<SourceGetDto>
        {
            Count = sources.Length,
            Items = sources
        };
    }
    public async Task<SourceGetDto> GetAsync(string companyId, byte sourceId)
    {
        companyId = companyId.ToUpperInvariant();
        var source = await companySourceRepo.FindAsync(companyId, sourceId);

        return source is null
            ? throw new NullReferenceException(nameof(source))
            : new SourceGetDto(sourceId, source.Source.Name, source.Value);
    }
    public async Task<CompanySource[]> CreateUpdateDeleteAsync(string companyId, IEnumerable<SourcePostDto> models)
    {
        companyId = companyId.ToUpperInvariant();
        return await companySourceRepo.CreateUpdateDeleteAsync(
            models
                .Select(x => new CompanySource
                {
                    CompanyId = companyId,
                    SourceId = x.Id,
                    Value = x.Value
                }),
            new CompanySourceComparer(),
            companyId);
    }
}