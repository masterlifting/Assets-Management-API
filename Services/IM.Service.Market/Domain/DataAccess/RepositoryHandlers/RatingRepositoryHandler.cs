using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Settings;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;
using IM.Service.Shared.RepositoryService;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using static IM.Service.Shared.Enums;

namespace IM.Service.Market.Domain.DataAccess.RepositoryHandlers;

public class RatingRepositoryHandler : RepositoryHandler<Rating>
{
    private readonly DatabaseContext context;
    private readonly string rabbitConnectionString;
    public RatingRepositoryHandler(IOptions<ServiceSettings> options, DatabaseContext context)
    {
        this.context = context;
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
    }

    public override async Task<IEnumerable<Rating>> RunUpdateRangeHandlerAsync(IReadOnlyCollection<Rating> entities)
    {
        var existEntities = await GetExist(entities).ToArrayAsync();

        var result = existEntities
            .Join(entities, x => x.CompanyId, y => y.CompanyId, (x, y) => (Old: x, New: y))
            .ToArray();

        foreach (var (Old, New) in result)
        {
            Old.ResultCoefficient = New.ResultCoefficient;
            Old.ResultDividend = New.ResultDividend;
            Old.ResultPrice = New.ResultPrice;
            Old.ResultReport = New.ResultReport;

            Old.Result = New.Result;
        }

        return result.Select(x => x.Old);
    }
    public override IQueryable<Rating> GetExist(IEnumerable<Rating> entities)
    {
        entities = entities.ToArray();

        var companyIds = entities
            .GroupBy(x => x.CompanyId)
            .Select(x => x.Key);

        return context.Rating.Where(x => companyIds.Contains(x.CompanyId));
    }

    public override Task RunPostProcessRangeAsync(RepositoryActions action, IReadOnlyCollection<Rating> entities)
    {
        var ratings = entities.OrderByDescending(x => x.Result).Select((x, i) => new RatingMqDto(x.CompanyId, i + 1));

        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);
        publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Ratings, RabbitHelper.GetQueueAction(action), ratings);

        return Task.CompletedTask;
    }
}