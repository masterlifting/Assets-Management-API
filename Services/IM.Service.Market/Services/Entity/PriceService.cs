using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data;
using IM.Service.Market.Services.Helpers;
using IM.Service.Market.Services.RabbitMq;
using IM.Service.Shared.Models.RabbitMq.Api;
using IM.Service.Shared.RabbitMq;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Entity;

public sealed class PriceService : StatusChanger<Price>
{
    private readonly RabbitAction rabbitAction;
    public DataLoader<Price> Loader { get; }
    private readonly Repository<Price> priceRepo;
    private readonly Repository<Split> splitRepo;

    public PriceService(RabbitAction rabbitAction, Repository<Price> priceRepo, Repository<Split> splitRepo, DataLoader<Price> loader) : base(priceRepo)
    {
        this.rabbitAction = rabbitAction;
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

        await TransferPriceAsync(price, action);
    }
    public async Task SetValueTrueAsync(QueueActions action, Price[] prices)
    {
        if (action is QueueActions.Delete)
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        var companyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var dateMax = prices.Max(x => x.Date);

        var splits = await splitRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId) && x.Date <= dateMax);

        if (!splits.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        foreach (var group in splits.GroupBy(x => x.CompanyId))
            ComputeValueTrue(action, group.Key, group, prices);

        await TransferPricesAsync(prices, action);
    }

    public async Task SetValueTrueAsync(QueueActions action, Split split)
    {
        var prices = await priceRepo.GetSampleAsync(x => x.CompanyId == split.CompanyId && x.Date >= split.Date);
        if (!prices.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        var splits = await splitRepo.GetSampleAsync(x => x.CompanyId == split.CompanyId);
        if (!splits.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        ComputeValueTrue(action, split.CompanyId, splits, prices);

        await TransferPricesAsync(prices, action);
    }
    public async Task SetValueTrueAsync(QueueActions action, Split[] splits)
    {
        var companyIds = splits.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var dateMin = splits.Min(x => x.Date);

        var prices = await priceRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId) && x.Date >= dateMin);
        if (!prices.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        var _splits = await splitRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId));
        if (!_splits.Any())
        {
            await TransferPricesAsync(prices, action);
            return;
        }

        foreach (var group in splits.GroupBy(x => x.CompanyId))
            ComputeValueTrue(action, group.Key, group, prices);

        await TransferPricesAsync(prices, action);
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

    private async Task TransferPriceAsync(Price entity, QueueActions action)
    {
        entity.StatusId = (byte)Statuses.Ready;
        await priceRepo.UpdateAsync(new object[] { entity.CompanyId, entity.SourceId, entity.Date }, entity, $"{nameof(SetValueTrueAsync)}: {entity.CompanyId}");

        var model = await GetPriceToTransferAsync(entity);

        rabbitAction.Publish(QueueExchanges.Transfer, QueueNames.Recommendations, QueueEntities.Cost, action, model);
    }
    private async Task TransferPricesAsync(Price[] entities, QueueActions action)
    {
        foreach (var entity in entities)
            entity.StatusId = (byte)Statuses.Ready;

        await priceRepo.UpdateRangeAsync(entities, "Change status to 'Ready'");

        var models = await GetPricesToTransferAsync(entities);

        rabbitAction.Publish(QueueExchanges.Transfer, QueueNames.Recommendations, QueueEntities.Costs, action, models);
    }

    private async Task<CostMqDto[]> GetPricesToTransferAsync(Price[] entities)
    {
        var companyIds = entities.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var sourceIds = entities.GroupBy(x => x.SourceId).Select(x => x.Key).ToArray();

        var date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-365));

        var splits = await splitRepo.GetSampleAsync(
            x => companyIds.Contains(x.CompanyId) && x.Date >= date,
            x => new { x.CompanyId, x.Date });

        var splitsDict = splits
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.MaxBy(z => z.Date)!.Date, StringComparer.OrdinalIgnoreCase);

        var prices = await priceRepo.GetSampleAsync(
            x => companyIds.Contains(x.CompanyId) && sourceIds.Contains(x.SourceId) && x.Date >= date,
            x => new { x.CompanyId, x.Date, x.Value });

        return prices
            .GroupBy(x => x.CompanyId)
            .Select(x =>
            {
                var data = splitsDict.ContainsKey(x.Key)
                    ? x.Where(y => y.Date >= splitsDict[x.Key]).ToArray()
                    : x.ToArray();

                return new CostMqDto(
                    x.Key,
                    (byte)Shared.Enums.AssetTypes.Stock,
                    data.MaxBy(y => y.Date)!.Value,
                    Math.Round(data.Average(y => y.Value), 4));
            })
            .ToArray();
    }
    private async Task<CostMqDto> GetPriceToTransferAsync(Price entity)
    {
        var date = DateOnly.FromDateTime(DateTime.Now.Date.AddDays(-365));

        var prices = await priceRepo.GetSampleAsync(
            x => entity.CompanyId.Equals(x.CompanyId, StringComparison.OrdinalIgnoreCase) && entity.SourceId.Equals(x.SourceId) && x.Date >= date,
            x => new { x.CompanyId, x.Date, x.Value });

        var splits = await splitRepo.GetSampleAsync(
            x =>x.CompanyId.Equals(entity.CompanyId, StringComparison.OrdinalIgnoreCase) && x.Date >= date,
            x => new { x.CompanyId, x.Date });

        var splitsDict = splits
            .GroupBy(x => x.CompanyId)
            .ToDictionary(x => x.Key, y => y.MaxBy(z => z.Date)!.Date, StringComparer.OrdinalIgnoreCase);

        var data = splitsDict.Any()
            ? prices.Where(y => y.Date >= splitsDict[entity.CompanyId]).ToArray()
            : prices;

        return new CostMqDto(
            entity.CompanyId,
            (byte)Shared.Enums.AssetTypes.Stock,
            data.MaxBy(y => y.Date)!.Value,
            Math.Round(data.Average(y => y.Value), 4));
    }
}