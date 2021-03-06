using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Models.Api.Http;
using IM.Service.Shared.Models.Service;

namespace IM.Service.Market.Services.Http;

public class CompanySourceApi
{
    private readonly Repository<CompanySource> companySourceRepo;
    public CompanySourceApi(Repository<CompanySource> companySourceRepo) => this.companySourceRepo = companySourceRepo;

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
                })
                .ToArray(),
            new CompanySourceComparer(),
            companyId);
    }
}