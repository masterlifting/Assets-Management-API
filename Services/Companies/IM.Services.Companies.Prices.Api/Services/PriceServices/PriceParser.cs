using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.MapServices;
using IM.Services.Companies.Prices.Api.Services.PriceServices.Implementations;
using IM.Services.Companies.Prices.Api.Services.PriceServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;

namespace IM.Services.Companies.Prices.Api.Services.PriceServices
{
    public class PriceParser
    {
        private readonly Dictionary<PriceSourceTypes, IPriceParser> parser;
        public PriceParser(MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient, PriceMapper priceMapper) =>
            parser = new()
            {
                { PriceSourceTypes.MOEX, new MoexParser(moexClient, priceMapper) },
                { PriceSourceTypes.Tdameritrade, new TdameritradeParser(tdAmeritradeClient, priceMapper) }
            };
        public async Task<Price[]> GetLastPricesToAddAsync(PriceSourceTypes sourceType, IEnumerable<(string ticker, DateTime priceDate)> data) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetLastPricesToAddAsync(data)
            : Array.Empty<Price>();
        public async Task<Price[]> GetLastPricesToUpdateAsync(PriceSourceTypes sourceType, IEnumerable<(string ticker, DateTime priceDate)> data) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetLastPricesToUpdateAsync(data)
            : Array.Empty<Price>();
        public async Task<Price[]> GetHistoryPricesAsync(PriceSourceTypes sourceType, IEnumerable<(string ticker, DateTime priceDate)> data) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetHistoryPricesAsync(data)
            : Array.Empty<Price>();
    }
}
