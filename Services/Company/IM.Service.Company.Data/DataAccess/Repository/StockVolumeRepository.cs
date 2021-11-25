using System.Collections.Generic;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Comparators;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class StockVolumeRepository : IRepositoryHandler<StockVolume>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public StockVolumeRepository(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public Task GetCreateHandlerAsync(ref StockVolume entity)
    {
        var exist = GetExist(new[] { entity });

        if (!exist.Any())
            return Task.CompletedTask;

        var comparer = new StockVolumeComparer();
        var checkedResult = new[] { entity }.Except(exist, comparer);

        if (checkedResult.Any())
            throw new ConstraintException($"Sotock volume with value: {entity.Value} is already. ");

        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref StockVolume[] entities)
    {
        var comparer = new StockVolumeComparer();
        entities = entities.Distinct(comparer).ToArray();

        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref StockVolume entity)
    {
        var ctxEntity = context.StockVolumes.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(StockVolume)} data not found. ");

        var exist = GetExist(new[] { entity }).ToArrayAsync().GetAwaiter().GetResult();
        var comparer = new StockVolumeComparer();
        var checkedResult = exist.Except(new[] { entity }, comparer).ToArray();

        if (checkedResult.Length > 1)
            throw new ConstraintException($"Sotock volume with value: {entity.Value} is already. ");

        ctxEntity.Value = entity.Value;
        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref StockVolume[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => (x.CompanyId, x.Date), y => (y.CompanyId, y.Date),
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
            Old.Value = New.Value;

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public async Task<IList<StockVolume>> GetDeleteHandlerAsync(IReadOnlyCollection<StockVolume> entities)
    {
        var comparer = new StockVolumeComparer();
        var result = new List<StockVolume>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.StockVolumes.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(StockVolume entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Price
            , QueueActions.Create
            , JsonSerializer.Serialize(new PriceIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = entity.Date
            }));

        return Task.CompletedTask;
    }
    public Task SetPostProcessAsync(IReadOnlyCollection<StockVolume> entities)
    {
        if (entities.Any())
        {
            var volume = entities.OrderBy(x => x.Date).First();
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Create
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = volume.CompanyId,
                    Date = volume.Date
                }));
        }

        return Task.CompletedTask;
    }

    private IQueryable<StockVolume> GetExist(StockVolume[] entities)
    {
        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.StockVolumes.Where(x => existData.Contains(x.CompanyId));
    }
}