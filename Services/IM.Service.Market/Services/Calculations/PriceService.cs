using System.Runtime.Serialization;

using IM.Service.Common.Net.RabbitServices;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;

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

    public async Task SetValueTrueAsync(string data)
    {
        if (!RabbitHelper.TrySerialize(data, out Split? split))
            throw new SerializationException(nameof(Split));

        var splits = await splitRepo.GetSampleAsync(x =>
            x.CompanyId == split!.CompanyId
            && x.Date >= split.Date);

        await SetValuesAsync(split!.CompanyId, splits);
    }
    public async Task SetValueTrueRangeAsync(string data)
    {
        if (!RabbitHelper.TrySerialize(data, out Split[]? splits))
            throw new SerializationException(nameof(Split));

        foreach (var group in splits!.GroupBy(x => x.CompanyId))
        {
            var _splits = await splitRepo.GetSampleAsync(x =>
                x.CompanyId == group.Key
                && x.Date >= group.Min(y => y.Date));

            await SetValuesAsync(group.Key, _splits);
        }
    }

    private async Task SetValuesAsync(string comapnyId, IEnumerable<Split> splits)
    {
        var splitData = splits.OrderBy(x => x.Date).Select(x => (x.Date, x.Value)).ToArray();
        var targetData = new List<(DateOnly, DateOnly, int)>(splitData.Length - 1);

        var splitValue = 0;

        for (var i = 1; i <= splitData.Length; i++)
        {
            splitValue += splitData[i - 1].Value;
            targetData.Add((splitData[i - 1].Date, splitData[i].Date, splitValue));
        }

        foreach (var (dateStart, dateEnd, value) in targetData)
        {
            var prices = await priceRepo.GetSampleAsync(x =>
                x.CompanyId == comapnyId
                && x.Date >= dateStart && x.Date < dateEnd);

            foreach (var item in prices)
                item.ValueTrue = item.Value * value;

            await priceRepo.UpdateAsync(prices, nameof(SetValueTrueAsync));
        }
    }
}