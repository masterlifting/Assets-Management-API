using IM.Service.Market.Domain.Entities.ManyToMany;

namespace IM.Service.Market.Services.DataLoaders;

public class DataGrabber : IDataGrabber
{
    private readonly Dictionary<byte, IDataGrabber> sourceMatcher;
    public DataGrabber(Dictionary<byte, IDataGrabber> sourceMatcher) => this.sourceMatcher = sourceMatcher;

    public bool ToContinue(CompanySource companySource) => sourceMatcher.ContainsKey(companySource.SourceId);
    public bool ToContinue(IEnumerable<CompanySource> companySources) => companySources
        .ToDictionary(x => x.SourceId)
        .Any(x => sourceMatcher.ContainsKey(x.Key));

    public Task GetCurrentDataAsync(CompanySource companySource) =>
        sourceMatcher.ContainsKey(companySource.SourceId)
            ? sourceMatcher[companySource.SourceId].GetCurrentDataAsync(companySource)
            : Task.CompletedTask;
    public Task GetCurrentDataAsync(IEnumerable<CompanySource> companySources)
    {
        foreach (var (sourceId, cs) in companySources.ToDictionary(x => x.SourceId).Where(x => sourceMatcher.ContainsKey(x.Key)))
            sourceMatcher[sourceId].GetCurrentDataAsync(cs);

        return Task.CompletedTask;
    }

    public Task GetHistoryDataAsync(CompanySource companySource) =>
        sourceMatcher.ContainsKey(companySource.SourceId)
            ? sourceMatcher[companySource.SourceId].GetHistoryDataAsync(companySource)
            : Task.CompletedTask;
    public Task GetHistoryDataAsync(IEnumerable<CompanySource> companySources)
    {
        foreach (var (sourceId, cs) in companySources.ToDictionary(x => x.SourceId).Where(x => sourceMatcher.ContainsKey(x.Key)))
            sourceMatcher[sourceId].GetHistoryDataAsync(cs);

        return Task.CompletedTask;
    }
}