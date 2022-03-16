using IM.Service.MarketData.Clients;
using IM.Service.MarketData.Domain.DataAccess;
using IM.Service.MarketData.Domain.Entities;
using IM.Service.MarketData.Domain.Entities.ManyToMany;
using IM.Service.MarketData.Services.DataLoaders.Prices.Implementations;

namespace IM.Service.MarketData.Services.DataLoaders.Prices;

public class PriceLoader : DataLoader<Price>
{
    public PriceLoader(
        ILogger<DataLoader<Price>> logger, 
        Repository<Price> repository, 
        Repository<CompanySource> companySourceRepo,
        MoexClient moexClient,
        TdAmeritradeClient tdAmeritradeClient) 
        : base(logger, repository , companySourceRepo, new Dictionary<byte, IDataGrabber>
        {
            { (byte)Enums.Sources.Moex, new MoexGrabber(repository, logger, moexClient) },
            { (byte)Enums.Sources.Tdameritrade, new TdameritradeGrabber(repository, logger, tdAmeritradeClient) }
        })
    {
        IsCurrentDataCondition = x => x.Date == GetLastWorkDay(x.SourceId);
        TimeAgo = 30;
    }

    private static readonly Dictionary<byte, DateOnly[]> ExchangeWeekend = new()
    {
        {
            (byte)Enums.Sources.Moex,
            new DateOnly[]
            {
                new(2021, 06, 14),
            }
        },
        {
            (byte)Enums.Sources.Tdameritrade,
            new DateOnly[]
            {
                new(2021, 05, 31),
            }
        }
    };
    private static DateOnly GetLastWorkDay(byte sourceId, DateOnly? date = null)
    {
        return CheckWorkday(sourceId, date ?? DateOnly.FromDateTime(DateTime.UtcNow));

        static DateOnly CheckWorkday(byte sourceId, DateOnly checkingDate) =>
            IsExchangeWeekend(sourceId, checkingDate)
                ? CheckWorkday(sourceId, checkingDate.AddDays(-1))
                : checkingDate;
    }
    private static bool IsExchangeWeekend(byte sourceId, DateOnly chackingDate) =>
        chackingDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
        && ExchangeWeekend.ContainsKey(sourceId)
        && ExchangeWeekend[sourceId].Contains(chackingDate);
}