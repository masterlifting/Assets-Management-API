﻿using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class StockSplitRepository : RepositoryHandler<StockSplit, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public StockSplitRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<StockSplit>> GetUpdateRangeHandlerAsync(IEnumerable<StockSplit> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => (x.CompanyId, x.Date),
                y => (y.CompanyId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Value = New.Value;

        return result.Select(x => x.Old).ToArray();
    }
    public override async Task<IEnumerable<StockSplit>> GetDeleteRangeHandlerAsync(IEnumerable<StockSplit> entities)
    {
        var comparer = new CompanyDateComparer<StockSplit>();
        var result = new List<StockSplit>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.StockSplits.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public override Task SetPostProcessAsync(StockSplit entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Price
            , QueueActions.CreateUpdate
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = entity.Date
            }));

        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(IReadOnlyCollection<StockSplit> entities)
    {
        if (!entities.Any())
            return Task.CompletedTask;

        var data = JsonSerializer.Serialize(entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x
                .OrderBy(y => y.Date)
                .First())
            .Select(x => new CompanyDateIdentityDto
            {
                CompanyId = x.CompanyId,
                Date = x.Date
            })
            .ToArray());

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Prices
            , QueueActions.CreateUpdate
            , data);

        return Task.CompletedTask;
    }

    public override IQueryable<StockSplit> GetExist(IEnumerable<StockSplit> entities)
    {
        entities = entities.ToArray();
        var dateMin = entities.Min(x => x.Date);
        var dateMax = entities.Max(x => x.Date);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.StockSplits.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
    }
}