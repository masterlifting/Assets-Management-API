using IM.Services.Companies.Prices.Api.Models.Client.MoexModels;
using IM.Services.Companies.Prices.Api.Models.Client.TdAmeritradeModels;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace IM.Services.Companies.Prices.Api.Services.MapServices
{
    public class PriceMapper
    {
        public Price[] MapToPrices(TdAmeritradeLastPriceResultModel clientResult) => clientResult.Data.Select(x => new Price()
        {
            Date = DateTimeOffset.FromUnixTimeMilliseconds(x.Value.regularMarketTradeTimeInLong).DateTime.Date,
            Value = x.Value.lastPrice,
            TickerName = x.Key.ToUpperInvariant()
        }).ToArray();
        public Price[] MapToPrices(TdAmeritradeHistoryPriceResultModel clientResult) => clientResult.Data.candles.Select(x => new Price()
        {
            Date = DateTimeOffset.FromUnixTimeMilliseconds(x.datetime).DateTime.Date,
            Value = x.high,
            TickerName = clientResult.Ticker.ToUpperInvariant()
        }).ToArray();
        public Price[] MapToPrices(MoexLastPriceResultModel clientResult, IEnumerable<string> tickers)
        {
            var prepareData = clientResult.Data.Marketdata.Data.Select(x => new
            {
                ticker = x[0].ToString(),
                date = x[48]?.ToString(),
                value = x[12]?.ToString()
            });
            var tickersData = prepareData.Where(x => x.value != null).Join(tickers, x => x.ticker, y => y, (x, y) => new
            {
                Ticker = y,
                Date = x.date,
                Price = x.value
            }).ToArray();

            if (!tickersData.Any())
                return Array.Empty<Price>();

            var result = new Price[tickersData.Length];

            for (int i = 0; i < tickersData.Length; i++)
                result[i] = new()
                {
                    Date = DateTime.Parse(tickersData[i].Date).Date,
                    Value = decimal.Parse(tickersData[i].Price, CultureInfo.InvariantCulture),
                    TickerName = tickersData[i].Ticker
                };

            return result;
        }
        public Price[] MapToPrices(MoexHistoryPriceResultModel clientResult)
        {
            var tickerData = clientResult.Data.History.Data;

            if (tickerData is null)
                return Array.Empty<Price>();

            var result = new List<Price>();
            for (int i = 0; i < tickerData.Length; i++)
            {
                if (tickerData[i][8] is not null)
                {
                    result.Add(new()
                    {
                        TickerName = clientResult.Ticker.ToUpperInvariant(),
                        Date = DateTime.Parse(tickerData[i][1].ToString()).Date,
                        Value = decimal.Parse(tickerData[i][8].ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
                    });
                }
            }

            return result.ToArray();
        }
    }
}