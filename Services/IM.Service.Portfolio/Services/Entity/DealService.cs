using System.Runtime.Serialization;
using System.Threading.Tasks;

using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RabbitMQ.Configuration;
using IM.Service.Portfolio.Domain.DataAccess;
using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Services.Entity;

public class DealService
{
    private readonly Repository<Deal> dealRepository;
    public DealService(Repository<Deal> dealRepository) => this.dealRepository = dealRepository;

    public async Task SetSummaryAsync(string data, QueueActions actions)
    {
        if (!JsonHelper.TryDeserialize(data, out Deal? deal))
            throw new SerializationException(nameof(Deal));

        var dbDeals = await dealRepository.GetSampleAsync(x =>
            x.AccountName == deal!.AccountName
            && x.AccountUserId == deal.AccountUserId
            && x.AccountBrokerId == deal.AccountBrokerId
            && x.DerivativeId == deal.DerivativeId);
    }
    public Task SetSummaryRangeAsync(string data, QueueActions actions)
    {
        if (!JsonHelper.TryDeserialize(data, out Deal[]? deals))
            throw new SerializationException(nameof(Deal));

        return Task.CompletedTask;
    }
}