using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Comparators;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.Data.Prices.Implementations;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.Data.Prices;

public sealed class PriceLoader : DataLoader<Price>
{
    private static readonly Dictionary<byte, DateOnly[]> exchangeWeekend = new()
    {
        {
            (byte)Sources.Moex,
            new DateOnly[]
            {
                new(2021, 06, 14),
            }
        },
        {
            (byte)Sources.Tdameritrade,
            new DateOnly[]
            {
                new(2021, 05, 31),
            }
        }
    };
    public PriceLoader(ILogger<PriceLoader> logger, Repository<Price> repository, MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient)
        : base(logger, repository, new Dictionary<byte, IDataGrabber<Price>>
        {
            { (byte)Sources.Moex, new MoexGrabber(moexClient) },
            { (byte)Sources.Tdameritrade, new TdameritradeGrabber(tdAmeritradeClient) }
        },
        isCurrentDataCondition: x => IsCurrentData(x.SourceId, x.Date),
        timeAgo: 30,
        comparer: new DataDateComparer<Price>())
    {
    }

    private static bool IsCurrentData(byte sourceId, DateOnly date)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        return currentDate == GetLastWorkday(sourceId, date);

        DateOnly GetLastWorkday(byte key, DateOnly checkingDate) =>
            IsExchangeRestday(key, checkingDate)
                ? GetLastWorkday(key, checkingDate.AddDays(-1))
                : checkingDate;
    }
    private static bool IsExchangeRestday(byte key, DateOnly chackingDate) =>
        chackingDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
        && exchangeWeekend.ContainsKey(key)
        && exchangeWeekend[key].Contains(chackingDate);
}