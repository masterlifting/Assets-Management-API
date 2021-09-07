using CommonServices.Models.Entity;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using IM.Services.Companies.Prices.Api.Services.MapServices;
using IM.Services.Companies.Prices.Api.Services.PriceServices.Implementations;
using IM.Services.Companies.Prices.Api.Services.PriceServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Services.Companies.Prices.Api.Enums;

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
        public async Task<Price[]> GetLastPricesToAddAsync(PriceSourceTypes sourceType, IEnumerable<PriceIdentity> prices) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetLastPricesToAddAsync(prices)
            : Array.Empty<Price>();
        public async Task<Price[]> GetLastPricesToUpdateAsync(PriceSourceTypes sourceType, IEnumerable<PriceIdentity> prices) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetLastPricesToUpdateAsync(prices)
            : Array.Empty<Price>();
        public async Task<Price[]> GetHistoryPricesAsync(PriceSourceTypes sourceType, IEnumerable<PriceIdentity> prices) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetHistoryPricesAsync(prices)
            : Array.Empty<Price>();
    }
}
