using CommonServices.Models.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.MapServices;
using IM.Service.Company.Prices.Services.PriceServices.Implementations;
using IM.Service.Company.Prices.Services.PriceServices.Interfaces;
using static IM.Service.Company.Prices.Enums;

namespace IM.Service.Company.Prices.Services.PriceServices
{
    public class PriceParser
    {
        private readonly Dictionary<Enums.PriceSourceTypes, IPriceParser> parser;
        public PriceParser(MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient, PriceMapper priceMapper) =>
            parser = new()
            {
                { Enums.PriceSourceTypes.MOEX, new MoexParser(moexClient, priceMapper) },
                { Enums.PriceSourceTypes.Tdameritrade, new TdameritradeParser(tdAmeritradeClient, priceMapper) }
            };
        public async Task<Price[]> GetLastPricesToAddAsync(Enums.PriceSourceTypes sourceType, IEnumerable<PriceIdentity> prices) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetLastPricesToAddAsync(prices)
            : Array.Empty<Price>();
        public async Task<Price[]> GetLastPricesToUpdateAsync(Enums.PriceSourceTypes sourceType, IEnumerable<PriceIdentity> prices) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetLastPricesToUpdateAsync(prices)
            : Array.Empty<Price>();
        public async Task<Price[]> GetHistoryPricesAsync(Enums.PriceSourceTypes sourceType, IEnumerable<PriceIdentity> prices) =>
            parser.ContainsKey(sourceType)
            ? await parser[sourceType].GetHistoryPricesAsync(prices)
            : Array.Empty<Price>();
    }
}
