using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Services.DataLoaders.Prices.Implementations;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.DataLoaders.Prices;

public class PriceLoader : DataLoader<Price>
{
    private readonly Dictionary<byte, DateOnly[]> exchangeWeekend;
    public PriceLoader(
        ILogger<DataLoader<Price>> logger, 
        Repository<Price> repository, 
        MoexClient moexClient,
        TdAmeritradeClient tdAmeritradeClient) 
        : base(logger, repository , new Dictionary<byte, IDataGrabber>
        {
            { (byte)Sources.Moex, new MoexGrabber(repository, logger, moexClient) },
            { (byte)Sources.Tdameritrade, new TdameritradeGrabber(repository, logger, tdAmeritradeClient) }
        })
    {
        exchangeWeekend = new()
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
        IsCurrentDataCondition = x => IsCurrentData(x.SourceId, x.Date);
        TimeAgo = 30;
    }

    private bool IsCurrentData(byte sourceId, DateOnly date)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);

        return currentDate == GetLastWorkday(sourceId, date);

        DateOnly GetLastWorkday(byte key, DateOnly checkingDate) =>
            IsExchangeRestday(key, checkingDate)
                ? GetLastWorkday(key, checkingDate.AddDays(-1))
                : checkingDate;
    }
    private bool IsExchangeRestday(byte key, DateOnly chackingDate) =>
        chackingDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
        && exchangeWeekend.ContainsKey(key)
        && exchangeWeekend[key].Contains(chackingDate);
}