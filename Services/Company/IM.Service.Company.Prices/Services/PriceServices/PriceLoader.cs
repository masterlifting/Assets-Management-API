using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Entity;
using CommonServices.RabbitServices;

using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.DataAccess.Repository;
using IM.Service.Company.Prices.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using static CommonServices.CommonHelper;

namespace IM.Service.Company.Prices.Services.PriceServices
{
    public class PriceLoader
    {
        private readonly RepositorySet<Price> repository;
        private readonly PriceParser parser;
        private readonly string rabbitConnectionString;
        public PriceLoader(IOptions<ServiceSettings> options, RepositorySet<Price> repository, PriceParser parser)
        {
            this.repository = repository;
            this.parser = parser;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<Price[]> LoadAsync(Ticker ticker)
        {
            var result = Array.Empty<Price>();
            var ctxTicker = await repository.GetDbSetBy<Ticker>().FindAsync(ticker.Name);

            if (ctxTicker is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ticker '{ticker.Name}' not found");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            if (ctxTicker.SourceValue is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Source value for '{ticker.Name}' not found");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            var source = await repository.GetDbSetBy<SourceType>().FindAsync(ctxTicker.SourceTypeId);
            var isWeekend = IsExchangeWeekend(source.Name, DateTime.UtcNow);

            if (isWeekend)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Exchange for '{ticker.Name}' has a day off");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            var lastPrice = await GetLastPriceAsync(ctxTicker);

            PriceIdentity data = lastPrice is not null
                ? new()
                {
                    TickerName = ctxTicker.SourceValue,
                    Date = lastPrice.Date
                }
                : new()
                {
                    TickerName = ctxTicker.SourceValue,
                    Date = DateTime.UtcNow.AddYears(-1)
                };

            var prices = await GetPricesAsync(source.Name, data);

            result = await SavePricesAsync(prices);

            if (result.Length <= 0)
                return result;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Saved data for {ticker.Name}");
            Console.ForegroundColor = ConsoleColor.Gray;
            return result;
        }
        public async Task<Price[]> LoadAsync()
        {
            var lastPrices = await GetLastPricesAsync();
            var lastPricesDic = lastPrices.ToDictionary(x => x.TickerName, y => y.Date);

            var tickers = await repository.GetDbSetBy<Ticker>()
                .Where(x => x.SourceValue != null)
                .Select(x => new { x.Name, x.SourceValue, x.SourceTypeId })
                .ToArrayAsync();

            var sources = await repository.GetDbSetBy<SourceType>()
                .Select(x => new { x.Id, x.Name })
                .ToArrayAsync();

            var result = new List<Price>(tickers.Length * 300);

            foreach (var item in tickers
                .GroupBy(x => x.SourceTypeId)
                .Join(sources, x => x.Key, y => y.Id, (x, y) =>
                new { Source = y.Name, Tickers = x }))
            {
                var data = item.Tickers
                    .Select(x => lastPricesDic.ContainsKey(x.Name)
                        ? new PriceIdentity { TickerName = x.SourceValue!, Date = lastPricesDic[x.Name].Date }
                        : new PriceIdentity { TickerName = x.SourceValue!, Date = DateTime.UtcNow.AddYears(-1) })
                    .ToArray();

                if (!data.Any())
                    continue;

                var prices = await GetPricesAsync(item.Source, data);
                result.AddRange(await SavePricesAsync(prices));
            }

            if (result.Count <= 0)
                return Array.Empty<Price>();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Saved data for '{result.Count}' tickers");
            Console.ForegroundColor = ConsoleColor.Gray;
            return result.ToArray();
        }

        private async Task<Price?> GetLastPriceAsync(TickerIdentity ticker)
        {
            var prices = await repository.GetSampleAsync(x => x.TickerName == ticker.Name && x.Date >= DateTime.UtcNow.AddMonths(-1));
            return prices
                .OrderBy(x => x.Date)
                .LastOrDefault();
        }
        private async Task<Price[]> GetLastPricesAsync()
        {
            var prices = await repository.GetSampleAsync(x => x.Date >= DateTime.UtcNow.AddMonths(-1));
            return prices
                .GroupBy(x => x.TickerName)
                .Select(x =>
                    x.OrderBy(y => y.Date)
                    .Last())
                .ToArray();
        }

        private async Task<Price[]> GetPricesAsync(string source, PriceIdentity data)
        {
            var result = await parser.LoadLastPricesAsync(source, data);

            if (data.Date < GetExchangeWorkDate(source))
                result = result.Concat(await parser.LoadHistoryPricesAsync(source, data)).ToArray();

            return result;
        }
        private async Task<Price[]> GetPricesAsync(string source, PriceIdentity[] data)
        {
            var exchangeDate = GetExchangeWorkDate(source);

            var dataToHistory = data.Where(x => x.Date < exchangeDate).ToArray();

            var result = await parser.LoadLastPricesAsync(source, data);

            if (dataToHistory.Any())
                result = result.Concat(await parser.LoadHistoryPricesAsync(source, dataToHistory)).ToArray();

            return result;
        }

        private async Task<Price[]> SavePricesAsync(IEnumerable<Price> prices)
        {
            var (errors, result) = await repository.CreateUpdateAsync(prices, new PriceComparer(), "prices");

            if (errors.Any())
                return result;

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            foreach (var price in result)
                publisher.PublishTask(
                    QueueNames.CompanyAnalyzer
                    , QueueEntities.Price
                    , QueueActions.SetLogic
                    , JsonSerializer.Serialize(new PriceGetDto
                    {
                        TickerName = price.TickerName,
                        SourceType = price.SourceType,
                        Date = price.Date
                    }));

            return result;
        }
    }
}