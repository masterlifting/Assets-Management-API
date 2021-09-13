using CommonServices.Models.Entity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Services.Company.Prices.DataAccess;
using IM.Services.Company.Prices.DataAccess.Entities;
using IM.Services.Company.Prices.DataAccess.Repository;
using static CommonServices.CommonHelper;
using static IM.Services.Company.Prices.Enums;

namespace IM.Services.Company.Prices.Services.PriceServices
{
    public class PriceLoader
    {
        private readonly PricesContext context;
        private readonly RepositorySet<Price> repository;
        private readonly PriceParser parser;
        public PriceLoader(PricesContext context, RepositorySet<Price> repository, PriceParser parser)
        {
            this.context = context;
            this.repository = repository;
            this.parser = parser;
        }

        public async Task<int> LoadPricesAsync()
        {
            if (IsExchangeWeekend(DateTime.UtcNow))
                return 0;

            var lastPrices = await GetLastPricesAsync(1);

            (List<Price> toUpdate, List<Price> toAdd) moexPrices = await GetPricesAsync(Enums.PriceSourceTypes.MOEX, lastPrices);
            (List<Price> toUpdate, List<Price> toAdd) tdameritradePrices = await GetPricesAsync(Enums.PriceSourceTypes.Tdameritrade, lastPrices);

            var moexSavedResult = await SavePricesAsync(moexPrices.toUpdate, moexPrices.toAdd);
            var tdameritradeSavedResult = await SavePricesAsync(tdameritradePrices.toUpdate, tdameritradePrices.toAdd);

            return moexSavedResult.Length + tdameritradeSavedResult.Length;
        }
        public async Task<Price[]> LoadPricesAsync(Ticker ticker)
        {
            var result = Array.Empty<Price>();

            var ctxTicker = await context.Tickers.FindAsync(ticker.Name);

            if (ctxTicker is not null
                && !IsExchangeWeekend(DateTime.UtcNow)
                && Enum.TryParse(ctxTicker.SourceTypeId.ToString(), true, out Enums.PriceSourceTypes sourceType))
            {
                var lastPrices = await GetLastPricesAsync(1);
                var (toUpdate, toAdd) = await GetPricesAsync(sourceType, lastPrices);

                result = await SavePricesAsync(toUpdate, toAdd);
            }

            Console.WriteLine($"\nSaved price count: {result.Length}");

            return result;
        }

        private async Task<Price[]> GetLastPricesAsync(int priceMonthsAgo)
        {
            var dbPrices = context.Prices;
            var dbTickers = context.Tickers;

            var targetPrices = dbPrices.Where(x => x.Date >= DateTime.UtcNow.AddMonths(-priceMonthsAgo));

            var prices = await dbTickers.Join(targetPrices, x => x.Name, y => y.TickerName, (_, y) => y).ToListAsync();
            var tickers = await dbTickers.Select(x => x.Name).ToArrayAsync();

            var lastPrices = GetLast(prices);
            await CheckTickersWithoutPricesAsync();

            return lastPrices;

            static Price[] GetLast(IEnumerable<Price> items) => items.GroupBy(x => x.TickerName).Select(x => x.OrderBy(y => y.Date.Date).LastOrDefault()).ToArray();
            async Task CheckTickersWithoutPricesAsync()
            {
                var tickersWithoutPrices = tickers.Except(lastPrices.Select(x => x.TickerName)).ToArray();

                if (tickersWithoutPrices.Any())
                {
                    var oldPrices = await dbPrices.Join(dbTickers.Where(x => tickersWithoutPrices.Contains(x.Name)), x => x.TickerName, y => y.Name, (x, _) => x).ToArrayAsync();
                    if (oldPrices.Any())
                    {
                        lastPrices = lastPrices.Concat(GetLast(oldPrices)).ToArray();
                        tickersWithoutPrices = tickersWithoutPrices.Except(oldPrices.Select(x => x.TickerName)).ToArray();
                    }

                    var defaultPrices = tickersWithoutPrices.Select(x => new Price { TickerName = x, Date = DateTime.UtcNow.AddYears(-1).Date }).ToList();
                    lastPrices = lastPrices.Concat(defaultPrices).ToArray();
                }
            }
        }
        private async Task<(List<Price> toUpdate, List<Price> toAdd)> GetPricesAsync(Enums.PriceSourceTypes sourceType, IEnumerable<Price> lastPrices)
        {
            var sourceTypeId = (byte)sourceType;
            var tickerNames = await context.Tickers.Where(x => x.SourceTypeId == sourceTypeId).Select(x => x.Name).ToArrayAsync();
            var prices = lastPrices.Join(tickerNames, x => x.TickerName, y => y, (x, _) => x).ToArray();
            var separatedPrices = GetSeparatedPrices(sourceType, prices);
            var pricesResult = await GetPricesResultAsync(sourceType, separatedPrices);

            return pricesResult;

            (IEnumerable<Price> forHistory, IEnumerable<Price> forLastToUpdate, IEnumerable<Price> forLastToAdd) GetSeparatedPrices(Enums.PriceSourceTypes sourceType, IEnumerable<Price> prices)
            {
                var lastWorkday = GetExchangeLastWorkday(sourceType.ToString());

                var pricesForHistory = prices.Where(x => x.Date < lastWorkday);
                var pricesForLastToUpdate = prices.Where(x => x.Date == DateTime.UtcNow.Date);
                var pricesForLastToAddNames = prices.Select(x => x.TickerName).Except(pricesForLastToUpdate.Select(x => x.TickerName));
                var pricesForLastToAdd = prices.Join(pricesForLastToAddNames, x => x.TickerName, y => y, (x, _) => x);

                return (pricesForHistory, pricesForLastToUpdate, pricesForLastToAdd);
            }
            async Task<(List<Price> toUpdate, List<Price> toAdd)> GetPricesResultAsync(Enums.PriceSourceTypes sourceType, (IEnumerable<Price> forHistory, IEnumerable<Price> forLastToUpdate, IEnumerable<Price> forLastToAdd) separated)
            {
                var toAdd = new List<Price>();
                var toUpdate = new List<Price>();

                if (separated.forHistory.Any())
                {
                    var data = separated.forHistory.Select(x => new PriceIdentity { TickerName = x.TickerName, Date = x.Date });
                    var dataResult = await parser.GetHistoryPricesAsync(sourceType, data);
                    toAdd.AddRange(dataResult);
                }

                if (separated.forLastToAdd.Any())
                {
                    var data = separated.forLastToAdd.Select(x => new PriceIdentity { TickerName = x.TickerName, Date = x.Date });
                    var dataResult = await parser.GetLastPricesToAddAsync(sourceType, data);
                    toAdd.AddRange(dataResult);
                }

                if (separated.forLastToUpdate.Any())
                {
                    var data = separated.forLastToUpdate.Select(x => new PriceIdentity { TickerName = x.TickerName, Date = x.Date });
                    var dataResult = await parser.GetLastPricesToUpdateAsync(sourceType, data);
                    toUpdate.AddRange(dataResult);
                }

                return (toUpdate, toAdd);
            }
        }
        private async Task<Price[]> SavePricesAsync(IReadOnlyCollection<Price> toUpdate, IReadOnlyCollection<Price> toAdd)
        {
            var result = new Price[toUpdate.Count + toAdd.Count];

            if (toAdd.Any())
            {
                var (errors, prices) = await repository.CreateAsync(toAdd, new PriceComparer(), "loaded prices");

                if (!errors.Any())
                    result = prices;
            }

            if (toUpdate.Any())
            {
                var (errors, prices) = await repository.UpdateAsync(toUpdate, "updated prices");
                if (!errors.Any())
                    result = result.Concat(prices).ToArray();
            }

            return result;
        }
    }
}