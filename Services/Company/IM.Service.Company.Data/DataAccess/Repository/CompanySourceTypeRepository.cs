using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class CompanySourceTypeRepository : IRepositoryHandler<CompanySourceType>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;

    public CompanySourceTypeRepository(
        IOptions<ServiceSettings> options,
        DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public Task GetCreateHandlerAsync(ref CompanySourceType entity)
    {
        return Task.CompletedTask;
    }
    public Task GetCreateHandlerAsync(ref CompanySourceType[] entities, IEqualityComparer<CompanySourceType> comparer)
    {
        var exist = GetExist(entities);

        if (exist.Any())
            entities = entities.Except(exist, comparer).ToArray();

        return Task.CompletedTask;
    }
        
    public Task GetUpdateHandlerAsync(ref CompanySourceType entity)
    {
        var ctxEntity = context.CompanySourceTypes.FindAsync(entity.CompanyId, entity.SourceTypeId).GetAwaiter().GetResult();

        ctxEntity.CompanyId = entity.CompanyId;
        ctxEntity.SourceTypeId = entity.SourceTypeId;
        ctxEntity.Value = entity.Value;

        entity = ctxEntity;

        return Task.CompletedTask;
    }
    public Task GetUpdateHandlerAsync(ref CompanySourceType[] entities)
    {
        var exist = GetExist(entities).ToArrayAsync().GetAwaiter().GetResult();

        var result = exist
            .Join(entities, x => new { x.CompanyId, x.SourceTypeId }, y => new { y.CompanyId, y.SourceTypeId },
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.CompanyId = New.CompanyId;
            Old.SourceTypeId = New.SourceTypeId;
            Old.Value = New.Value;
        }

        entities = result.Select(x => x.Old).ToArray();

        return Task.CompletedTask;
    }

    public Task SetPostProcessAsync(CompanySourceType entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Report, QueueActions.Call, entity.CompanyId);
        publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Price, QueueActions.Call, entity.CompanyId);

        return Task.CompletedTask;
    }
    public Task SetPostProcessAsync(CompanySourceType[] entities)
    {
        var companyIds = entities.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);

        foreach (var companyId in companyIds)
        {
            publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Report, QueueActions.Call, companyId);
            publisher.PublishTask(QueueNames.CompanyData, QueueEntities.Price, QueueActions.Call, companyId);
        }

        return Task.CompletedTask;
    }

    private IQueryable<CompanySourceType> GetExist(IEnumerable<CompanySourceType> entities)
    {
        var existData = entities
            .Where(x => x.Value is not null)
            .GroupBy(x => new { x.CompanyId, x.SourceTypeId, x.Value })
            .Select(x => x.Key)
            .ToArray();

        return context.CompanySourceTypes.Where(x => existData.Contains(new { x.CompanyId, x.SourceTypeId, x.Value }));
    }
}