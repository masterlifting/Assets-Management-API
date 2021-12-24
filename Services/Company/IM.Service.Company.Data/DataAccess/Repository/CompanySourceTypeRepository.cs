using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;
using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class CompanySourceTypeRepository : RepositoryHandler<CompanySourceType, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public CompanySourceTypeRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<CompanySourceType>> GetUpdateRangeHandlerAsync(IEnumerable<CompanySourceType> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.SourceTypeId),
                y => (y.CompanyId, y.SourceTypeId),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.CompanyId = New.CompanyId;
            Old.SourceTypeId = New.SourceTypeId;
            Old.Value = New.Value;
        }

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<CompanySourceType>> GetDeleteRangeHandlerAsync(IEnumerable<CompanySourceType> entities)
    {
        var comparer = new CompanySourceTypeComparer();
        var result = new List<CompanySourceType>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.CompanySourceTypes.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override Task SetPostProcessAsync(CompanySourceType entity)
    {
        if (entity.Value is null)
            return Task.CompletedTask;

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        SetQueue(entity.SourceTypeId, entity.CompanyId, publisher);

        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(IReadOnlyCollection<CompanySourceType> entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        foreach (var entity in entities.Where(x => x.Value is not null))
            SetQueue(entity.SourceTypeId, entity.CompanyId, publisher);

        return Task.CompletedTask;
    }

    public override IQueryable<CompanySourceType> GetExist(IEnumerable<CompanySourceType> entities)
    {
        var existData = entities
            .Where(x => x.Value is not null)
            .GroupBy(x => (x.CompanyId, x.SourceTypeId, x.Value ))
            .Select(x => new CompanySourceType
            {
                CompanyId = x.Key.CompanyId,
                SourceTypeId = x.Key.SourceTypeId,
                Value = x.Key.Value
            })
            .ToArray();

        var companyIds = existData.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var typeIds = existData.GroupBy(x => x.SourceTypeId).Select(x => x.Key).ToArray();
        var values = existData.GroupBy(x => x.Value).Select(x => x.Key).ToArray();

        var dbc = context.CompanySourceTypes.Where(x => companyIds.Contains(x.CompanyId));
        var dbt = context.CompanySourceTypes.Where(x => typeIds.Contains(x.SourceTypeId));

        return dbc
            .Join(dbt, x => new { x.CompanyId, x.SourceTypeId }, y => new { y.CompanyId, y.SourceTypeId }, (x, _) => x)
            .Where(x => values.Contains(x.Value));
    }

    private static void SetQueue(byte sourceTypeId, string companyId, RabbitPublisher publisher)
    {
        if (!Enum.TryParse<SourceTypes>(sourceTypeId.ToString(), out var sourceType))
            return;

        switch (sourceType)
        {
            case SourceTypes.Official:
                {
                    break;
                }
            case SourceTypes.Moex or SourceTypes.Tdameritrade:
                {
                    publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Price, QueueActions.Call, companyId);
                    break;
                }
            case SourceTypes.Investing:
                {
                    publisher.PublishTask(QueueNames.CompanyData, QueueEntities.CompanyReport, QueueActions.Call, companyId);
                    publisher.PublishTask(QueueNames.CompanyData, QueueEntities.StockVolume, QueueActions.Call, companyId);
                    break;
                }
        }
    }
}