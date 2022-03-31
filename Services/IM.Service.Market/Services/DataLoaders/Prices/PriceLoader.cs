using IM.Service.Market.Clients;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.ManyToMany;
using IM.Service.Market.Services.DataLoaders.Prices.Implementations;

namespace IM.Service.Market.Services.DataLoaders.Prices;

public class PriceLoader : DataLoader<Price>
{
    private readonly Dictionary<byte, DateOnly[]> exchangeWeekend;
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
        exchangeWeekend = new()
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
    }

    private DateOnly GetLastWorkDay(byte sourceId, DateOnly? date = null)
    {
        return CheckWorkday(sourceId, date ?? DateOnly.FromDateTime(DateTime.UtcNow));

        DateOnly CheckWorkday(byte key, DateOnly checkingDate) =>
            IsExchangeWeekend(key, checkingDate)
                ? CheckWorkday(key, checkingDate.AddDays(-1))
                : checkingDate;
    }
    private bool IsExchangeWeekend(byte key, DateOnly chackingDate) =>
        chackingDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
        && exchangeWeekend.ContainsKey(key)
        && exchangeWeekend[key].Contains(chackingDate);
}