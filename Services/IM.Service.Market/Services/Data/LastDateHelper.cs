using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.Data;

public sealed class LastDateHelper<TEntity> : ILastDataHelper<TEntity> where TEntity : class, IDataIdentity, IDateIdentity
{
    private readonly Repository<TEntity> repository;
    private readonly int daysAgo;

    public LastDateHelper(Repository<TEntity> repository, int daysAgo)
    {
        this.repository = repository;
        this.daysAgo = daysAgo;
    }

    public async Task<TEntity?> GetLastDataAsync(CompanySource companySource)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-daysAgo);

        var result = await repository.GetSampleOrderedAsync(x =>
                x.CompanyId == companySource.CompanyId
                && x.SourceId == companySource.SourceId
                && x.Date >= date,
            orderBy => orderBy.Date);

        return result.LastOrDefault();
    }
    public async Task<TEntity[]> GetLastDataAsync(CompanySource[] companySources)
    {
        var companyIds = companySources.Select(x => x.CompanyId).Distinct();
        var sourceIds = companySources.Select(x => x.SourceId).Distinct();
        var date = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-daysAgo);

        var result = await repository.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && sourceIds.Contains(x.SourceId)
            && x.Date >= date);

        return result
            .GroupBy(x => (x.CompanyId, x.SourceId))
            .Select(x => x.OrderBy(y => y.Date).Last())
            .ToArray();
    }
}