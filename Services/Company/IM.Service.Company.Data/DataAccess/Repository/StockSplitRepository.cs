using System.Collections.Generic;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class StockSplitRepository : IRepositoryHandler<StockSplit>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public StockSplitRepository(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public Task GetCreateHandlerAsync(ref StockSplit entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref StockSplit[] entities)
    {
        var comparer = new CompanyDateComparer<StockSplit>();
        entities = entities.Distinct(comparer).ToArray();
        
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref StockSplit entity)
    {
        var ctxEntity = context.StockSplits.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(StockSplit)} data not found. ");

        ctxEntity.Value = entity.Value;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref StockSplit[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => (CompanyId: x.CompanyId, x.Date), y => (CompanyId: y.CompanyId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Value = New.Value;

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<StockSplit>> GetDeleteHandlerAsync(IReadOnlyCollection<StockSplit> entities)
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

    public Task SetPostProcessAsync(StockSplit entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Price
            , QueueActions.Create
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = entity.Date
            }));

        return Task.CompletedTask;
    }
    public Task SetPostProcessAsync(IReadOnlyCollection<StockSplit> entities)
    {
        if (entities.Any())
        {
            var split = entities.OrderBy(x => x.Date).First();
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Create
                , JsonSerializer.Serialize(new CompanyDateIdentityDto
                {
                    CompanyId = split.CompanyId,
                    Date = split.Date
                }));
        }

        return Task.CompletedTask;
    }

    private IQueryable<StockSplit> GetExist(StockSplit[] entities)
    {
        var dateMin = entities.Min(x => x.Date);
        var dateMax = entities.Max(x => x.Date);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.StockSplits.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
    }
}