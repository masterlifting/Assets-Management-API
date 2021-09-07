using CommonServices.Models.Entity;
using CommonServices.RepositoryService;

using IM.Services.Companies.Prices.Api.DataAccess;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonHelper;
using static IM.Services.Companies.Prices.Api.Enums;

namespace IM.Services.Companies.Prices.Api.Services.PriceServices
{
    public class PriceLoader
    {
        private readonly PricesContext context;
        private readonly PricesRepository<Price> repository;
        private readonly PriceParser parser;
        public PriceLoader(PricesContext context, PricesRepository<Price> repository, PriceParser parser)
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
            var moexPrices = await GetPricesAsync(PriceSourceTypes.MOEX, lastPrices);
            var tdameritradePrices = await GetPricesAsync(PriceSourceTypes.Tdameritrade, lastPrices);
            var savedPrices = await SavePricesAsync(new[] { moexPrices, tdameritradePrices });
            return savedPrices.Length;
        }
        public async Task<Price[]> LoadPricesAsync(Ticker ticker)
        {
            var result = Array.Empty<Price>();

            var _ticker = await context.Tickers.FindAsync(ticker.Name);

            if (_ticker is not null
                && !IsExchangeWeekend(DateTime.UtcNow)
                && Enum.TryParse(_ticker.SourceTypeId.ToString(), out PriceSourceTypes sourceType))
            {
                var lastPrices = await GetLastPricesAsync(1);
                var (toUpdate, toAdd) = await GetPricesAsync(sourceType, lastPrices);

                result = new Price[toUpdate.Count + toAdd.Count];

                if (toAdd.Any())
                    result = await repository.CreateAsync(toAdd, new PriceComparer(), "loaded prices");

                if (toUpdate.Any())
                {
                    var updatedResult = await repository.UpdateAsync(toUpdate, "updated prices");
                    result = result.Concat(updatedResult).ToArray();
                }
            }

            Console.WriteLine($"\nSaved price count: {result.Length}");

            return result;
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
            byte sourceTypeId = (byte)sourceType;
            var tickerNames = await context.Tickers.Where(x => x.SourceTypeId == sourceTypeId).Select(x => x.Name).ToArrayAsync();
            var prices = lastPrices.Join(tickerNames, x => x.TickerName, y => y, (x, _) => x).ToArray();
            var separatedPrices = GetSeparatedPrices(sourceType, prices);
            var pricesResult = await GetPricesResultAsync(sourceType, separatedPrices);

            return pricesResult;

            (IEnumerable<Price> forHistory, IEnumerable<Price> forLastToUpdate, IEnumerable<Price> forLastToAdd) GetSeparatedPrices(PriceSourceTypes sourceType, IEnumerable<Price> prices)
            {
                var lastWorkday = GetExchangeLastWorkday(sourceType.ToString());

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
        private async Task<Price[]> SavePricesAsync((List<Price> toUpdate, List<Price> toAdd)[] data)
        {
            var result = Array.Empty<Price>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].toAdd.Count > 0)
                {
                    await context.Prices.AddRangeAsync(data[i].toAdd);
                    result = result.Concat(data[i].toAdd).ToArray();
                }

                if (data[i].toUpdate.Count > 0)
                {
                    for (int j = 0; j < data[i].toUpdate.Count; j++)
                    {
                        var price = await context.Prices.FindAsync(data[i].toUpdate[j].TickerName, data[i].toUpdate[j].Date);
                        price.Value = data[i].toUpdate[j].Value;
                    }
                    result = result.Concat(data[i].toUpdate).ToArray();
                }
            }

            await context.SaveChangesAsync();

            return result;
        }
    }
}