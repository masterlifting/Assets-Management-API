using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.DataLoaders;

public class DataGrabber<TEntity> : IDataGrabber<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    private readonly Dictionary<byte, IDataGrabber<TEntity>> sourceMatcher;
    public DataGrabber(Dictionary<byte, IDataGrabber<TEntity>> sourceMatcher) => this.sourceMatcher = sourceMatcher;

    public bool ToContinue(CompanySource companySource) => sourceMatcher.ContainsKey(companySource.SourceId);
    public bool ToContinue(IEnumerable<CompanySource> companySources) => companySources
        .ToDictionary(x => x.SourceId)
        .Any(x => sourceMatcher.ContainsKey(x.Key));

    public async IAsyncEnumerable<TEntity[]> GetCurrentDataAsync(CompanySource companySource)
    {
        if (!sourceMatcher.ContainsKey(companySource.SourceId)) 
            yield break;
        await foreach (var data in sourceMatcher[companySource.SourceId].GetCurrentDataAsync(companySource))
            yield return data;
    }
    public async IAsyncEnumerable<TEntity[]> GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        var enumerableData = companySources
            .ToDictionary(x => x.SourceId)
            .Where(x => sourceMatcher.ContainsKey(x.Key))
            .ToArray();

        foreach (var (sourceId, cs) in enumerableData)
            await foreach (var entities in sourceMatcher[sourceId].GetCurrentDataAsync(cs))
                yield return entities;
    }

    public async IAsyncEnumerable<TEntity[]> GetHistoryDataAsync(CompanySource companySource)
    {
        if (!sourceMatcher.ContainsKey(companySource.SourceId))
            yield break;
        await foreach (var data in sourceMatcher[companySource.SourceId].GetHistoryDataAsync(companySource))
            yield return data;
    }
    public async IAsyncEnumerable<TEntity[]> GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        var enumerableData = companySources
            .ToDictionary(x => x.SourceId)
            .Where(x => sourceMatcher.ContainsKey(x.Key))
            .ToArray();

        foreach (var (sourceId, cs) in enumerableData)
            await foreach (var entities in sourceMatcher[sourceId].GetHistoryDataAsync(cs))
                yield return entities;
    }
}