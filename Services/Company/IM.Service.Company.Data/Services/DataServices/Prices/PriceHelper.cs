using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Client.Price.MoexModels;
using IM.Service.Company.Data.Models.Client.Price.TdAmeritradeModels;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.Prices;

public static class PriceHelper
{
    public static class PriceMapper
    {
        public static Price[] Map(string source, TdAmeritradeLastPriceResultModel clientResult) =>
            clientResult.Data is null
                ? Array.Empty<Price>()
                : clientResult.Data.Select(x => new Price
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(x.Value.regularMarketTradeTimeInLong).DateTime.Date,
                    Value = x.Value.lastPrice,
                    CompanyId = x.Key,
                    SourceType = source
                }).ToArray();
        public static Price[] Map(string source, TdAmeritradeHistoryPriceResultModel clientResult) =>
            clientResult.Data?.candles is null
            ? Array.Empty<Price>()
            : clientResult.Data.candles.Select(x => new Price
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds(x.datetime).DateTime.Date,
                Value = x.high,
                CompanyId = clientResult.Ticker,
                SourceType = source
            }).ToArray();
        public static Price[] Map(string source, MoexCurrentPriceResultModel clientResult, IEnumerable<string> tickers)
        {
            var clientData = clientResult.Data.Marketdata.Data;

            var prepareData = clientData.Select(x => new
            {
                ticker = x[0].ToString(),
                date = x[48],
                price = x[12]
            })
            .Where(x => x.ticker != null);

            var tickersData = prepareData.Join(tickers, x => x.ticker, y => y, (x, y) => new
            {
                Ticker = y,
                Date = x.date.ToString(),
                Price = x.price.ToString()
            }).ToArray();

            var result = new Price[tickersData.Length];

            for (var i = 0; i < tickersData.Length; i++)
                if (DateTime.TryParse(tickersData[i].Date, out var date)
                    && decimal.TryParse(tickersData[i].Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price))
                    result[i] = new()
                    {
                        Date = date.Date,
                        Value = price,
                        CompanyId = tickersData[i].Ticker,
                        SourceType = source
                    };

            return result;
        }
        public static Price[] Map(string source, MoexHistoryPriceResultModel clientResult)
        {
            var (moexHistoryPriceData, ticker) = clientResult;

            var clientData = moexHistoryPriceData.History.Data;

            var result = new List<Price>();

            foreach (var data in clientData)
            {
                var priceObject = data?[8];
                var dateObject = data?[1];

                if ((DateTime.TryParse(dateObject?.ToString(), out var date)
                     && decimal.TryParse(priceObject?.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price)))
                    result.Add(new()
                    {
                        CompanyId = ticker,
                        Date = date.Date,
                        Value = price,
                        SourceType = source
                    });
            }

            return result.ToArray();
        }
    }
    public static class ExchangeInfo
    {
        private static readonly Dictionary<string, DateOnly[]> ExchangeWeekend = new(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                nameof(SourceTypes.Moex),
                new DateOnly[]
                {
                    new(2021, 06, 14),
                }
            },
            {
                 nameof(SourceTypes.Tdameritrade),
                new DateOnly[]
                {
                    new(2021, 05, 31),
                }
            }
        };

        public static DateOnly GetLastWorkDay(string source, DateOnly? date = null)
        {
            return CheckWorkday(source, date ?? DateOnly.FromDateTime(DateTime.UtcNow));

            static DateOnly CheckWorkday(string source, DateOnly checkingDate) =>
                IsExchangeWeekend(source, checkingDate)
                    ? CheckWorkday(source, checkingDate.AddDays(-1))
                    : checkingDate;
        }

        private static bool IsExchangeWeekend(string source, DateOnly chackingDate) =>
            chackingDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday
            && ExchangeWeekend.ContainsKey(source)
            && ExchangeWeekend[source].Contains(chackingDate);
    }
}