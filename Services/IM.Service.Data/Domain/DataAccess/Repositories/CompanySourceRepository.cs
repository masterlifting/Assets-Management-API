﻿using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Data.Domain.DataAccess.Comparators;
using IM.Service.Data.Domain.Entities.ManyToMany;
using IM.Service.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Data.Domain.DataAccess.Repositories;

public class CompanySourceRepository : RepositoryHandler<CompanySource, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public CompanySourceRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<CompanySource>> GetUpdateRangeHandlerAsync(IEnumerable<CompanySource> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceId),
                y => (y.CompanyId, y.SourceId),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Value = New.Value;

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<CompanySource>> GetDeleteRangeHandlerAsync(IEnumerable<CompanySource> entities)
    {
        var comparer = new CompanySourceComparer();
        var result = new List<CompanySource>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.CompanySources.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override Task SetPostProcessAsync(RepositoryActions action, CompanySource entity)
    {
        if (entity.Value is null || action == RepositoryActions.Delete)
            return Task.CompletedTask;

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        SetQueueTask(entity.SourceId, entity.CompanyId, publisher);

        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(RepositoryActions action, IReadOnlyCollection<CompanySource> entities)
    {
        if (action == RepositoryActions.Delete)
            return Task.CompletedTask;

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        foreach (var entity in entities.Where(x => x.Value is not null))
            SetQueueTask(entity.SourceId, entity.CompanyId, publisher);

        return Task.CompletedTask;
    }

    public override IQueryable<CompanySource> GetExist(IEnumerable<CompanySource> entities)
    {
        entities = entities.ToArray();
        
        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var sourceIds = entities
            .GroupBy(x => x.SourceId)
            .Select(x => x.Key);

        return context.CompanySources
            .Where(x =>
                companyIds.Contains(x.CompanyId)
                && sourceIds.Contains(x.SourceId));
    }

    private static void SetQueueTask(byte sourceId, string companyId, RabbitPublisher publisher)
    {
        switch (sourceId)
        {
            case (byte)Enums.Sources.Official:
                    break;
            case (byte)Enums.Sources.Moex or (byte)Enums.Sources.Tdameritrade:
                {
                    publisher.PublishTask(QueueNames.MarketData, QueueEntities.Price, QueueActions.Call, companyId);
                    break;
                }
            case (byte)Enums.Sources.Investing:
                {
                    publisher.PublishTask(QueueNames.MarketData, QueueEntities.Report, QueueActions.Call, companyId);
                    publisher.PublishTask(QueueNames.MarketData, QueueEntities.StockVolume, QueueActions.Call, companyId);
                    break;
                }
        }
    }
}