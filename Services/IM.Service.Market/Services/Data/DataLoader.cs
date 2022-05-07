using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Domain.Entities.ManyToMany;

using System.Runtime.Serialization;

namespace IM.Service.Market.Services.Data;

public abstract class DataLoader<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    private readonly ILogger logger;
    private readonly ILastDataHelper<TEntity> lastDataHelper;

    private readonly Repository<TEntity> repository;
    private readonly DataGrabber<TEntity> grabber;

    private readonly Func<TEntity, bool> isCurrentDataCondition;
    private readonly IEqualityComparer<TEntity>? comparer;

    protected DataLoader(
        ILogger logger,
        Repository<TEntity> repository,
        Dictionary<byte, IDataGrabber<TEntity>> sourceMatcher,
        Func<TEntity, bool> isCurrentDataCondition,
        int timeAgo,
        IEqualityComparer<TEntity>? comparer
        )
    {
        this.logger = logger;
        this.repository = repository;
        this.comparer = comparer;
        this.isCurrentDataCondition = isCurrentDataCondition;

        grabber = new DataGrabber<TEntity>(sourceMatcher);

        lastDataHelper = repository switch
        {
            Repository<Price> repo => (new LastDateHelper<Price>(repo, timeAgo) as ILastDataHelper<TEntity>)!,
            Repository<Split> repo => (new LastDateHelper<Split>(repo, timeAgo) as ILastDataHelper<TEntity>)!,
            Repository<Float> repo => (new LastDateHelper<Float>(repo, timeAgo) as ILastDataHelper<TEntity>)!,
            Repository<Dividend> repo => (new LastDateHelper<Dividend>(repo, timeAgo) as ILastDataHelper<TEntity>)!,
            Repository<Report> repo => (new LastQuarterHelper<Report>(repo, timeAgo) as ILastDataHelper<TEntity>)!,
            _ => throw new ArgumentOutOfRangeException(nameof(repository), repository, null)
        };
    }

    public Task LoadAsync(string data) =>
        JsonHelper.TryDeserialize(data, out CompanySource? entity)
            ? DataSetAsync(entity!)
            : throw new SerializationException(typeof(TEntity).Name);
    public Task LoadRangeAsync(string data) =>
        JsonHelper.TryDeserialize(data, out CompanySource[]? entities)
            ? DataSetAsync(entities!)
            : throw new SerializationException(typeof(TEntity).Name);


    private async Task DataSetAsync(CompanySource companySource)
    {
        if (comparer is null)
            throw new NullReferenceException(nameof(comparer));

        if (!grabber.ToContinue(companySource))
            return;

        await foreach (var data in grabber.GetCurrentDataAsync(companySource))
            await repository.CreateUpdateRangeAsync(data, comparer, companySource.CompanyId);

        var last = await lastDataHelper.GetLastDataAsync(companySource);
        var isCurrent = last is not null && isCurrentDataCondition.Invoke(last);

        if (!isCurrent)
            await foreach (var data in grabber.GetHistoryDataAsync(companySource))
                await repository.UpdateRangeAsync(data, companySource.CompanyId);

        logger.LogDebug(nameof(DataSetAsync), typeof(TEntity).Name, companySource.CompanyId);
    }
    private async Task DataSetAsync(CompanySource[] companySources)
    {
        if (comparer is null)
            throw new NullReferenceException(nameof(comparer));

        if (!grabber.ToContinue(companySources))
            return;

        var lasts = await lastDataHelper.GetLastDataAsync(companySources);

        if (!lasts.Any())
            await foreach (var data in grabber.GetHistoryDataAsync(companySources))
                await repository.CreateRangeAsync(data, comparer, nameof(grabber.GetHistoryDataAsync) + ": " + string.Join(",", data.Select(x => x.CompanyId).Distinct()));
        else
        {
            var currentData = lasts.Where(x => isCurrentDataCondition.Invoke(x)).ToArray();
            var historyData = lasts.Where(x => !isCurrentDataCondition.Invoke(x)).ToArray();

            await foreach (var data in grabber.GetCurrentDataAsync(companySources.Join(currentData, x => (x.CompanyId, x.SourceId), y => (y.CompanyId, y.SourceId), (x, _) => x)))
                await repository.CreateUpdateRangeAsync(data, comparer, nameof(grabber.GetCurrentDataAsync) + ": " + string.Join(",", data.Select(x => x.CompanyId).Distinct()));

            await foreach (var data in grabber.GetHistoryDataAsync(companySources.Join(historyData, x => (x.CompanyId, x.SourceId), y => (y.CompanyId, y.SourceId), (x, _) => x)))
                await repository.CreateUpdateRangeAsync(data, comparer, nameof(grabber.GetHistoryDataAsync) + ": " + string.Join(",", data.Select(x => x.CompanyId).Distinct()));
        }

        logger.LogDebug(nameof(DataSetAsync), typeof(TEntity).Name, "OK");
    }
}

public interface ILastDataHelper<TEntity> where TEntity : class, IDataIdentity, IPeriod
{
    Task<TEntity?> GetLastDataAsync(CompanySource companySource);
    Task<TEntity[]> GetLastDataAsync(CompanySource[] companySources);
}
public sealed class LastQuarterHelper<TEntity> : ILastDataHelper<TEntity> where TEntity : class, IDataIdentity, IQuarterIdentity
{
    private readonly Repository<TEntity> repository;
    private readonly int yearsAgo;

    public LastQuarterHelper(Repository<TEntity> repository, int yearsAgo)
    {
        this.repository = repository;
        this.yearsAgo = yearsAgo;
    }

    public async Task<TEntity?> GetLastDataAsync(CompanySource companySource)
    {
        var result = await repository.GetSampleAsync(x =>
                x.CompanyId == companySource.CompanyId
                && x.SourceId == companySource.SourceId
                && x.Year >= DateTime.UtcNow.AddYears(-yearsAgo).Year);

        return result
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Quarter)
            .LastOrDefault();
    }
    public async Task<TEntity[]> GetLastDataAsync(CompanySource[] companySources)
    {
        var companyIds = companySources.Select(x => x.CompanyId).Distinct();
        var sourceIds = companySources.Select(x => x.SourceId).Distinct();

        var result = await repository.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && sourceIds.Contains(x.SourceId)
            && x.Year >= DateTime.UtcNow.AddYears(-yearsAgo).Year);

        return result
            .GroupBy(x => (x.CompanyId, x.SourceId))
            .Select(x =>
                x.OrderBy(y => y.Year)
                    .ThenBy(y => y.Quarter)
                    .Last())
            .ToArray();
    }
}
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
        var result = await repository.GetSampleOrderedAsync(x =>
            x.CompanyId == companySource.CompanyId
            && x.SourceId == companySource.SourceId
            && x.Date >= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-daysAgo),
            orderBy => orderBy.Date);

        return result.LastOrDefault();
    }
    public async Task<TEntity[]> GetLastDataAsync(CompanySource[] companySources)
    {
        var companyIds = companySources.Select(x => x.CompanyId).Distinct();
        var sourceIds = companySources.Select(x => x.SourceId).Distinct();

        var result = await repository.GetSampleAsync(x =>
            companyIds.Contains(x.CompanyId)
            && sourceIds.Contains(x.SourceId)
            && x.Date >= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-daysAgo));

        return result
            .GroupBy(x => (x.CompanyId, x.SourceId))
            .Select(x => x.OrderBy(y => y.Date).Last())
            .ToArray();
    }
}