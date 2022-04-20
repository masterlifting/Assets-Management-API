using IM.Service.Common.Net.Helpers;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;

using System.Runtime.Serialization;

using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Calculations;

public class PriceService
{
    private readonly Repository<Price> priceRepo;
    private readonly Repository<Split> splitRepo;

    public PriceService(Repository<Price> priceRepo, Repository<Split> splitRepo)
    {
        this.priceRepo = priceRepo;
        this.splitRepo = splitRepo;
    }

    public async Task SetValueTrueAsync<T>(string data, QueueActions action) where T : class, IDataIdentity
    {
        if (!JsonHelper.TryDeserialize(data, out T? entity))
            throw new SerializationException(typeof(T).Name);

        var result = entity switch
        {
            Price price => await SetValueAsync(action, price),
            Split split => await SetValueAsync(split),
            _ => throw new ArgumentOutOfRangeException($"{typeof(T).Name} not recognized")
        };

        foreach (var price in result)
        {
            price.StatusId = (byte) Statuses.Ready;
            if (price.ValueTrue == 0)
                price.ValueTrue = price.Value;
        }

        var (error, _) = await priceRepo.UpdateAsync(result, nameof(SetValueTrueAsync));

        if (error is not null)
            throw new Exception(error);
    }
    public async Task SetValueTrueRangeAsync<T>(string data, QueueActions action) where T : class, IDataIdentity
    {
        if (!JsonHelper.TryDeserialize(data, out T[]? entities))
            throw new SerializationException(typeof(T).Name);

        var result = entities switch
        {
            Price[] prices => await SetValueAsync(action, prices),
            Split[] splits => await SetValueAsync(splits),
            _ => throw new ArgumentOutOfRangeException($"{typeof(T).Name} not recognized")
        };

        foreach (var price in result)
        {
            price.StatusId = (byte)Statuses.Ready;
            if (price.ValueTrue == 0)
                price.ValueTrue = price.Value;
        }

        var (error, _) = await priceRepo.UpdateAsync(result, nameof(SetValueTrueRangeAsync));

        if (error is not null)
            throw new Exception(error);
    }

    private async Task<Price[]> SetValueAsync(QueueActions action, Price price)
    {
        if (action is QueueActions.Delete)
            return Array.Empty<Price>();

        var splits = await splitRepo.GetSampleAsync(x => x.CompanyId == price.CompanyId && x.Date <= price.Date);

        if (!splits.Any())
            return new[] { price };

        Compute(splits, price);

        return new[] { price };
    }
    private async Task<Price[]> SetValueAsync(QueueActions action, Price[] prices)
    {
        if (action is QueueActions.Delete)
            return Array.Empty<Price>();

        var companyIds = prices.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var dateMin = prices.Min(x => x.Date);
        var dateMax = prices.Max(x => x.Date);

        var splits = await splitRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId) && x.Date >= dateMin && x.Date <= dateMax);

        if (!splits.Any())
            return prices;

        foreach (var group in splits.GroupBy(x => x.CompanyId))
            Compute(group.Key, group, prices);

        return prices;
    }
    private async Task<Price[]> SetValueAsync(Split split)
    {
        var splits = await splitRepo.GetSampleAsync(x => x.CompanyId == split.CompanyId);

        if (!splits.Any())
            return Array.Empty<Price>();

        var prices = await priceRepo.GetSampleAsync(x => x.CompanyId == split.CompanyId && x.Date >= split.Date);

        Compute(split.CompanyId, splits, prices);

        return prices;
    }
    private async Task<Price[]> SetValueAsync(Split[] splits)
    {
        var companyIds = splits.GroupBy(x => x.CompanyId).Select(x => x.Key).ToArray();
        var dateMin = splits.Min(x => x.Date);
        var _splits = await splitRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId));

        if (!_splits.Any())
            return Array.Empty<Price>();

        var prices = await priceRepo.GetSampleAsync(x => companyIds.Contains(x.CompanyId) && x.Date >= dateMin);

        if (!prices.Any())
            return Array.Empty<Price>();

        foreach (var group in splits.GroupBy(x => x.CompanyId))
            Compute(group.Key, group, prices);

        return prices;
    }

    private static void Compute(IEnumerable<Split> splits, Price price)
    {
        var splitAgregatedValue = splits.OrderBy(x => x.Date).Select(x => x.Value).Aggregate((x, y) => x * y);
        price.ValueTrue = price.Value * splitAgregatedValue;
    }
    private static void Compute(string companyId, IEnumerable<Split> splits, IReadOnlyCollection<Price> prices)
    {
        var splitData = splits.OrderBy(x => x.Date).Select(x => (x.Date, x.Value)).ToArray();
        var targetData = new List<(DateOnly dateStart, DateOnly dateEnd, int value)>(splitData.Length);

        var splitValue = 0;

        if (splitData.Length > 1)
        {
            for (var i = 1; i <= splitData.Length; i++)
            {
                splitValue *= splitData[i - 1].Value;
                targetData.Add((splitData[i - 1].Date, splitData[i].Date, splitValue));
            }

            foreach (var (dateStart, dateEnd, value) in targetData)
                foreach (var price in prices.Where(x => x.CompanyId == companyId && x.Date >= dateStart && x.Date < dateEnd))
                    price.ValueTrue = price.Value * value;
        }
        else
        {
            splitValue = splitData[0].Value;
            var splitDate = splitData[0].Date;

            foreach (var price in prices.Where(x => x.CompanyId == companyId && x.Date >= splitDate))
                price.ValueTrue = price.Value * splitValue;
        }
    }
}