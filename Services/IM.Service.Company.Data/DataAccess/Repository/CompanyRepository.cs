using IM.Service.Common.Net.Models.Dto.Mq.CompanyServices;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Company.Data.DataAccess.Repository;

public class CompanyRepository : RepositoryHandler<Entities.Company, DatabaseContext>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public CompanyRepository(IOptions<ServiceSettings> options, DatabaseContext context) : base(context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<Entities.Company> GetUpdateHandlerAsync(object[] id, Entities.Company entity)
    {
        var dbEntity = await context.Companies.FindAsync(id);

        if (dbEntity is null)
            throw new SqlNullValueException(entity.Id);

        dbEntity.Name = entity.Name;
        dbEntity.IndustryId = entity.IndustryId;
        dbEntity.Description = entity.Description;

        return dbEntity;
    }
    public override async Task<IEnumerable<Entities.Company>> GetUpdateRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        entities = entities.ToArray();
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities,
                x => x.Id,
                y => y.Id,
                (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.Name = New.Name;
            Old.IndustryId = New.IndustryId;
            Old.Description = New.Description;
        }

        return result.Select(x => x.Old);
    }
    public override async Task<IEnumerable<Entities.Company>> GetDeleteRangeHandlerAsync(IEnumerable<Entities.Company> entities)
    {
        var comparer = new CompanyComparer<Entities.Company>();
        var ctxEntities = await context.Companies.ToArrayAsync();
        return ctxEntities.Except(entities, comparer);
    }

    public override Task SetPostProcessAsync(RepositoryActions action, Entities.Company entity)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync);

        var data = JsonSerializer.Serialize(new CompanyDto
        {
            Id = entity.Id,
            Name = entity.Name
        });

        publisher.PublishTask(new[]
             {
                 QueueNames.CompanyAnalyzer,
                 QueueNames.Recommendation,
                 QueueNames.BrokerData,
                 QueueNames.BrokerSummary
             },
            QueueEntities.Company,
            RabbitHelper.GetQueueAction(action),
            data);

        return Task.CompletedTask;
    }
    public override Task SetPostProcessAsync(RepositoryActions action, IReadOnlyCollection<Entities.Company> entities)
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Sync);

        var data = JsonSerializer.Serialize(entities.Select(x => new CompanyDto
        {
            Id = x.Id,
            Name = x.Name
        }));

        publisher.PublishTask(new[]
            {
                QueueNames.CompanyAnalyzer,
                QueueNames.Recommendation,
                QueueNames.BrokerData,
                QueueNames.BrokerSummary
            },
            QueueEntities.Company,
            RabbitHelper.GetQueueAction(action),
            data);

        return Task.CompletedTask;
    }

    public override IQueryable<Entities.Company> GetExist(IEnumerable<Entities.Company> entities)
    {
        var existData = entities
            .GroupBy(x => x.Id)
            .Select(x => x.Key)
            .ToArray();

        return context.Companies.Where(x => existData.Contains(x.Id));
    }
}