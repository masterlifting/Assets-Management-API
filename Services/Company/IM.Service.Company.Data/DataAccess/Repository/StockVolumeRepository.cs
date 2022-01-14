using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Comparators;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices.Configuration;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class StockVolumeRepository : RepositoryHandler<StockVolume, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public StockVolumeRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<StockVolume> GetCreateHandlerAsync(StockVolume entity)
    {
        var existEntities = await GetExist(new[] { entity }).ToArrayAsync();

        return existEntities.Any()
            ? throw new ConstraintException($"'{entity.Value}' for '{entity.CompanyId}' is already.")
            : entity;
    }
    public override async Task<IEnumerable<StockVolume>> GetUpdateRangeHandlerAsync(IEnumerable<StockVolume> entities)
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
    public override async Task<IEnumerable<StockVolume>> GetDeleteRangeHandlerAsync(IEnumerable<StockVolume> entities)
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

    public override Task SetPostProcessAsync(StockVolume entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);

        publisher.PublishTask(
            QueueNames.CompanyAnalyzer
            , QueueEntities.Coefficient
            , QueueActions.CreateUpdate
            , JsonSerializer.Serialize(new CompanyDateIdentityDto
            {
                CompanyId = entity.CompanyId,
                Date = entity.Date
            }));

        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(IReadOnlyCollection<StockVolume> entities)
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
            , QueueEntities.Coefficients
            , QueueActions.CreateUpdate
            , data);

        return Task.CompletedTask;
    }

    public override IQueryable<StockVolume> GetExist(IEnumerable<StockVolume> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);
        var values = entities
            .GroupBy(x => x.Value)
            .Select(x => x.Key);

        return context.StockVolumes.Where(x => companyIds.Contains(x.CompanyId) && values.Contains(x.Value));
    }
}