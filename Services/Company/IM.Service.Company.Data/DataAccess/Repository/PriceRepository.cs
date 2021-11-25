using System.Collections.Generic;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class PriceRepository : IRepositoryHandler<Price>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public PriceRepository(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public Task GetCreateHandlerAsync(ref Price entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref Price[] entities)
    {
        var comparer = new CompanyDateComparer<Price>();
        entities = entities.Distinct(comparer).ToArray();
        
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }

    public Task GetUpdateHandlerAsync(ref Price entity)
    {
        var ctxEntity = context.Prices.FindAsync(entity.CompanyId, entity.Date).GetAwaiter().GetResult();

        if (ctxEntity is null)
            throw new DataException($"{nameof(Price)} data not found. ");

        ctxEntity.Value = entity.Value;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref Price[] entities)
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

    public async Task<IList<Price>> GetDeleteHandlerAsync(IReadOnlyCollection<Price> entities)
    {
        var comparer = new CompanyDateComparer<Price>();
        var result = new List<Price>();

        foreach (var group in entities.GroupBy(x => x.CompanyId))
        {
            var dbEntities = await context.Prices.Where(x => x.CompanyId.Equals(group.Key)).ToArrayAsync();
            result.AddRange(dbEntities.Except(group, comparer));
        }

        return result;
    }

    public Task SetPostProcessAsync(Price entity)
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
    public Task SetPostProcessAsync(IReadOnlyCollection<Price> entities)
    {
        if (entities.Any())
        {
            var price = entities.OrderBy(x => x.Date).First();
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Create
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = price.CompanyId,
                    Date = price.Date
                }));
        }

        return Task.CompletedTask;
    }

    private IQueryable<Price> GetExist(Price[] entities)
    {
        var dateMin = entities.Min(x => x.Date);
        var dateMax = entities.Max(x => x.Date);

        var existData = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key)
            .ToArray();

        return context.Prices.Where(x => existData.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);
    }
}