using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.Mapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static IM.Services.Companies.Prices.Api.DataAccess.DataEnums;

namespace IM.Services.Companies.Prices.Api.Services.Background.Implementations
{
    public class PriceUpdater : IPriceUpdater
    {
        private readonly PricesContext context;
        private readonly Dictionary<PriceSourceTypes, IClientPriceUpdater> updaters;
        private static readonly Dictionary<PriceSourceTypes, DateTime[]> Weekend = new()
        {
            {
                PriceSourceTypes.moex,
                new DateTime[]
                {
                    new(2021, 06, 14),
                    new(2021, 11, 04),
                    new(2021, 11, 05),
                    new(2021, 12, 31)
                }
            },
            {
                PriceSourceTypes.tdameritrade,
                new DateTime[]
                {
                    new(2021, 05, 31),
                    new(2021, 06, 05),
                    new(2021, 09, 06),
                    new(2021, 10, 11),
                    new(2021, 11, 11),
                    new(2021, 11, 25),
                    new(2021, 12, 24),
                    new(2021, 12, 31)
                }
            }
        };

        public PriceUpdater(
            PricesContext context,
            MoexClient moexClient,
            TdAmeritradeClient tdAmeritradeClient,
            IPriceMapper priceMapper)
        {
            this.context = context;
            updaters = new()
            {
                { PriceSourceTypes.moex, new MoexPriceUpdater(moexClient, priceMapper) },
                { PriceSourceTypes.tdameritrade, new TdameritradePriceUpdater(tdAmeritradeClient, priceMapper) }
            };
        }

        public async Task<int> UpdatePricesAsync()
        {
            if (IsWeekend(DateTime.UtcNow))
                return 0;

            var lastPrices = await GetLastPricesAsync(1);
            var moexPrices = await GetPricesAsync(PriceSourceTypes.moex, lastPrices);
            var tdameritradePrices = await GetPricesAsync(PriceSourceTypes.tdameritrade, lastPrices);
            return await SavePricesAsync(new[] { moexPrices, tdameritradePrices });
        }

        private async Task<IEnumerable<Price>> GetLastPricesAsync(int priceMonthsAgo)
        {
            var dbPrices = context.Prices;
            var dbTickers = context.Tickers;

            var targetPrices = dbPrices.Where(x => x.Date >= DateTime.UtcNow.AddMonths(-priceMonthsAgo));

            var prices = await dbTickers.Join(targetPrices, x => x.Name, y => y.TickerName, (_, y) => y).ToListAsync();
            var tickers = await dbTickers.Select(x => x.Name).ToArrayAsync();

            var lastPrices = GetLast(prices);
            await CheckTickersWithoutPricesAsync();

            return lastPrices;

            static List<Price> GetLast(IEnumerable<Price> items) => items.GroupBy(x => x.TickerName).Select(x => x.OrderBy(y => y.Date.Date).LastOrDefault()).ToList();
            async Task CheckTickersWithoutPricesAsync()
            {
                var tickersWithoutPrices = tickers.Except(lastPrices.Select(x => x.TickerName));

                if (tickersWithoutPrices.Any())
                {
                    var oldPrices = await dbPrices.Join(dbTickers.Where(x => tickersWithoutPrices.Contains(x.Name)), x => x.TickerName, y => y.Name, (x, _) => x).ToArrayAsync();
                    if (oldPrices.Any())
                    {
                        lastPrices.AddRange(GetLast(oldPrices));
                        tickersWithoutPrices = tickersWithoutPrices.Except(oldPrices.Select(x => x.TickerName));
                    }

                    var defaultPrices = tickersWithoutPrices.Select(x => new Price() { TickerName = x, Date = DateTime.UtcNow.AddYears(-1).Date }).ToList();
                    lastPrices.AddRange(defaultPrices);
                }
            }
        }
        private async Task<(List<Price> toUpdate, List<Price> toAdd)> GetPricesAsync(PriceSourceTypes sourceType, IEnumerable<Price> lastPrices)
        {
            int sourceTypeId = (int)sourceType;
            var tickerNames = await context.Tickers.Where(x => x.PriceSourceTypeId == sourceTypeId).Select(x => x.Name).ToArrayAsync();
            var prices = lastPrices.Join(tickerNames, x => x.TickerName, y => y, (x, _) => x).ToArray();
            var separatedPrices = GetSeparatedPrices(sourceType, prices);
            var pricesResult = await GetPricesResultAsync(sourceType, separatedPrices);

            return pricesResult;

            (IEnumerable<Price> forHistory, IEnumerable<Price> forLastToUpdate, IEnumerable<Price> forLastToAdd) GetSeparatedPrices(PriceSourceTypes sourceType, IEnumerable<Price> prices)
            {
                var lastWorkday = GetLastWorkday(sourceType);

                var pricesForHistory = prices.Where(x => x.Date < lastWorkday);
                var pricesForLastToUpdate = prices.Where(x => x.Date == DateTime.UtcNow.Date);
                var pricesForLastToAddNames = prices.Select(x => x.TickerName).Except(pricesForLastToUpdate.Select(x => x.TickerName));
                var pricesForLastToAdd = prices.Join(pricesForLastToAddNames, x => x.TickerName, y => y, (x, _) => x);

                return (pricesForHistory, pricesForLastToUpdate, pricesForLastToAdd);
            }
            async Task<(List<Price> toUpdate, List<Price> toAdd)> GetPricesResultAsync(PriceSourceTypes sourceType, (IEnumerable<Price> forHistory, IEnumerable<Price> forLastToUpdate, IEnumerable<Price> forLastToAdd) separated)
            {
                var toAdd = new List<Price>();
                var toUpdate = new List<Price>();

                if (separated.forHistory.Any())
                {
                    var data = separated.forHistory.Select(x => (x.TickerName, x.Date));
                    var dataResult = await updaters[sourceType].GetHistoryPricesAsync(data);
                    toAdd.AddRange(dataResult);
                }

                if (separated.forLastToAdd.Any())
                {
                    var data = separated.forLastToAdd.Select(x => (x.TickerName, x.Date));
                    var dataResult = await updaters[sourceType].GetLastPricesToAddAsync(data);
                    toAdd.AddRange(dataResult);
                }

                if (separated.forLastToUpdate.Any())
                {
                    var data = separated.forLastToUpdate.Select(x => (x.TickerName, x.Date));
                    var dataResult = await updaters[sourceType].GetLastPricesToUpdateAsync(data);
                    toUpdate.AddRange(dataResult);
                }

                return (toUpdate, toAdd);
            }
        }
        private async Task<int> SavePricesAsync((List<Price> toUpdate, List<Price> toAdd)[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].toAdd.Count > 0)
                    await context.Prices.AddRangeAsync(data[i].toAdd);
                if (data[i].toUpdate.Count > 0)
                    for (int j = 0; j < data[i].toUpdate.Count; j++)
                    {
                        var price = await context.Prices.FindAsync(data[i].toUpdate[j].TickerName, data[i].toUpdate[j].Date);
                        price.Value = data[i].toUpdate[j].Value;
                    }
            }

            return await context.SaveChangesAsync();
        }

        private static bool IsWeekend(DateTime date) => date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday;
        private static bool IsWeekend(PriceSourceTypes sourceType, DateTime date) => Weekend[sourceType].Contains(date.Date);
        private static DateTime GetLastWorkday(PriceSourceTypes sourceType)
        {
            return CheckWorkday(sourceType, DateTime.UtcNow.AddDays(-1));

            static DateTime CheckWorkday(PriceSourceTypes sourceType, DateTime checkingDate) =>
                IsWeekend(checkingDate)
                ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                : IsWeekend(sourceType, checkingDate)
                    ? CheckWorkday(sourceType, checkingDate.AddDays(-1))
                    : checkingDate.Date;
        }
    }
}