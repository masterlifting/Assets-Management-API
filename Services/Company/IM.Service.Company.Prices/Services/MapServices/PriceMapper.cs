using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Models.Client.MoexModels;
using IM.Service.Company.Prices.Models.Client.TdAmeritradeModels;
// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace IM.Service.Company.Prices.Services.MapServices
{
    public static class PriceMapper
    {
        public static Price[] MapToPrices(string lowerSourceType, TdAmeritradeLastPriceResultModel clientResult) =>
            clientResult.Data is null
                ? Array.Empty<Price>()
                : clientResult.Data.Select(x => new Price
                {
                    Date = DateTimeOffset.FromUnixTimeMilliseconds(x.Value.regularMarketTradeTimeInLong).DateTime.Date,
                    Value = x.Value.lastPrice,
                    TickerName = x.Key.ToUpperInvariant(),
                    SourceType = lowerSourceType
                }).ToArray();
        public static Price[] MapToPrices(string lowerSourceType, TdAmeritradeHistoryPriceResultModel clientResult) =>
            clientResult.Data?.candles is null
            ? Array.Empty<Price>()
            : clientResult.Data.candles.Select(x => new Price
            {
                Date = DateTimeOffset.FromUnixTimeMilliseconds(x.datetime).DateTime.Date,
                Value = x.high,
                TickerName = clientResult.Ticker.ToUpperInvariant(),
                SourceType = lowerSourceType
            }).ToArray();
        public static Price[] MapToPrices(string lowerSourceType, MoexLastPriceResultModel clientResult, IEnumerable<string> tickers)
        {
            var clientData = clientResult.Data?.Marketdata?.Data;

            if (clientData is null)
                return Array.Empty<Price>();

            var prepareData = clientData.Select(x => new
            {
                ticker = x[0].ToString(),
                date = x[48].ToString(),
                value = x[12].ToString()
            })
            .Where(x => x.value != null);

            var tickersData = prepareData.Join(tickers, x => x.ticker, y => y, (x, y) => new
            {
                Ticker = y,
                Date = x.date,
                Price = x.value
            }).ToArray();

            var result = new Price[tickersData.Length];

            for (var i = 0; i < tickersData.Length; i++)
                if (DateTime.TryParse(tickersData[i].Date, out var date)
                    && decimal.TryParse(tickersData[i].Price, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price))
                    result[i] = new()
                    {
                        Date = date.Date,
                        Value = price,
                        TickerName = tickersData[i].Ticker,
                        SourceType = lowerSourceType
                    };

            return result;
        }
        public static Price[] MapToPrices(string lowerSourceType, MoexHistoryPriceResultModel clientResult)
        {
            var clientData = clientResult.Data?.History?.Data;

            if (clientData is null)
                return Array.Empty<Price>();

            var result = new List<Price>();

            foreach (var data in clientData)
            {
                var data8 = data[8].ToString();

                if (data8 is not null
                    && (DateTime.TryParse(data[1].ToString(), out var date)
                    && decimal.TryParse(data8, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price)))
                    result.Add(new()
                    {
                        TickerName = clientResult.Ticker.ToUpperInvariant(),
                        Date = date.Date,
                        Value = price,
                        SourceType = lowerSourceType
                    });
            }

            return result.ToArray();
        }
    }
}