using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Services.Helpers;
using IM.Service.Market.Settings;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.Entity;

public sealed class PriceService : StatusChanger<Price>
{
    private readonly string rabbitConnectionString;
    public DataLoader<Price> Loader { get; }
    private readonly Repository<Price> priceRepo;
    private readonly Repository<Split> splitRepo;

    public PriceService(IOptions<ServiceSettings> options, Repository<Price> priceRepo, Repository<Split> splitRepo, DataLoader<Price> loader) : base(priceRepo)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        Loader = loader;
        this.priceRepo = priceRepo;
        this.splitRepo = splitRepo;
    }

    public async Task SetValueTrueAsync(QueueActions action, Price price)
    {
        if (action is QueueActions.Delete)
        {
            await TransferPriceAsync(price, action);
            return;
        }

        var splits = await splitRepo.GetSampleAsync(x => x.CompanyId == price.CompanyId && x.Date <= price.Date);

        if (!splits.Any())
        {
            await TransferPriceAsync(price, action);
            return;
        }

        ComputeValueTrue(action, splits, price);

        await UpdatePriceAsync(price, action);
    }
    public async Task SetValueTrueAsync(QueueActions action, Price[] prices)
    {
        if (action is QueueActions.Delete)
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        var companyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var dateMin = prices.Min(x => x.Date);
        var dateMax = prices.Max(x => x.Date);

        var splits = await splitRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);

        if (!splits.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        foreach (var group in splits.GroupBy(x => x.CompanyId))
            ComputeValueTrue(action, group.Key, group, prices);

        await UpdatePricesAsync(prices, action);
    }

    public async Task SetValueTrueAsync(QueueActions action, Split split)
    {
        var splits = await splitRepo.GetSampleAsync(x => x.CompanyId == split.CompanyId);
        var prices = await priceRepo.GetSampleAsync(x => x.CompanyId == split.CompanyId && x.Date >= split.Date);

        if (!prices.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        if (!splits.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        ComputeValueTrue(action, split.CompanyId, splits, prices);

        await UpdatePricesAsync(prices, action);
    }
    public async Task SetValueTrueAsync(QueueActions action, Split[] splits)
    {
        var companyIds = splits.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var dateMin = splits.Min(x => x.Date);
        var _splits = await splitRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId));
        var prices = await priceRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId) && x.Date >= dateMin);

        if (!prices.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        if (!_splits.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        foreach (var group in splits.GroupBy(x => x.CompanyId))
            ComputeValueTrue(action, group.Key, group, prices);

        await UpdatePricesAsync(prices, action);
    }

    private static void ComputeValueTrue(QueueActions action, IEnumerable<Split> splits, Price price)
    {
        if (action is QueueActions.Delete)
        {
            price.ValueTrue = 0;
            return;
        }

        var splitAgregatedValue = splits.OrderBy(x => x.Date).Select(x => x.Value).Aggregate((x, y) => x * y);
        price.ValueTrue = price.Value * splitAgregatedValue;
    }
    private static void ComputeValueTrue(QueueActions action, string companyId, IEnumerable<Split> splits, IReadOnlyCollection<Price> prices)
    {
        var splitData = splits.OrderBy(x => x.Date).Select(x => (x.Date, x.Value)).ToArray();
        var targetData = new List<(DateOnly dateStart, DateOnly dateEnd, int value)>(splitData.Length);

        var splitValue = 0;

        if (splitData.Length > 1)
        {
            for (var i = 1; i <= splitData.Length; i++)
            {
                splitValue *= action is not QueueActions.Delete ? splitData[i - 1].Value : 0;
                targetData.Add((splitData[i - 1].Date, splitData[i].Date, splitValue));
            }

            foreach (var (dateStart, dateEnd, value) in targetData)
                foreach (var price in prices.Where(x => x.CompanyId == companyId && x.Date >= dateStart && x.Date < dateEnd))
                    price.ValueTrue = price.Value * value;
        }
        else
        {
            splitValue = action is not QueueActions.Delete ? splitData[0].Value : 0;
            var splitDate = splitData[0].Date;

            foreach (var price in prices.Where(x => x.CompanyId == companyId && x.Date >= splitDate))
                price.ValueTrue = price.Value * splitValue;
        }
    }

    private async Task UpdatePricesAsync(Price[] prices, QueueActions action)
    {
        await priceRepo.UpdateRangeAsync(prices, $"{nameof(SetValueTrueAsync)}: {string.Join("; ", prices.Select(x => x.CompanyId).Distinct())}").ConfigureAwait(false);
        await TransferPricesAsync(prices, action);
    }
    private async Task UpdatePriceAsync(Price price, QueueActions action)
    {
        await priceRepo.UpdateAsync(new object[] { price.CompanyId, price.SourceId, price.Date }, price, $"{nameof(SetValueTrueAsync)}: {price.CompanyId}");
        await TransferPriceAsync(price, action);
    }

    private async Task TransferPriceAsync(Price entity, QueueActions action)
    {
        var model = await GetPriceToTransferAsync(entity);
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);
        publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Price, action, model);
    }
    private async Task TransferPricesAsync(Price[] entities, QueueActions action)
    {
        var models = await GetPricesToTransferAsync(entities);
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Transfer);
        publisher.PublishTask(QueueNames.Recommendations, QueueEntities.Prices, action, models);
    }

    private async Task<PriceMqDto[]> GetPricesToTransferAsync(Price[] entities)
    {
        var companyIds = entities.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var sourceIds = entities.GroupBy(x => x.SourceId).Select(x => x.Key).ToArray();

        var date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-365));

        var prices = await priceRepo.GetSampleAsync(
            x => companyIds.Contains(x.CompanyId) && sourceIds.Contains(x.SourceId) && x.Date >= date,
            x => new { x.CompanyId, x.Date, x.ValueTrue });

        var result = prices
            .GroupBy(x => x.CompanyId)
            .Select(x => new PriceMqDto(
                x.Key,
                x.MaxBy(y => y.Date)!.ValueTrue,
                 Math.Round(x.Average(y => y.ValueTrue),4)))
            .ToArray();

        return result;
    }
    private async Task<PriceMqDto> GetPriceToTransferAsync(Price entity)
    {
        var date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-365));

        var prices = await priceRepo.GetSampleAsync(
            x => entity.CompanyId.Equals(x.CompanyId) && entity.SourceId.Equals(x.SourceId) && x.Date >= date,
            x => new { x.CompanyId, x.Date, x.ValueTrue });

        var result = new PriceMqDto(
                entity.CompanyId,
                prices.MaxBy(y => y.Date)!.ValueTrue,
                Math.Round(prices.Average(y => y.ValueTrue),4));

        return result;
    }
}