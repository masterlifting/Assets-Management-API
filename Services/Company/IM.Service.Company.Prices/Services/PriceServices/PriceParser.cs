using CommonServices.Models.Entity;

using IM.Service.Company.Prices.Clients;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.Services.PriceServices.Implementations;
using IM.Service.Company.Prices.Services.PriceServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Prices.Enums;

namespace IM.Service.Company.Prices.Services.PriceServices
{
    public class PriceParser
    {
        private readonly Dictionary<PriceSourceTypes, IPriceParser> parser;
        public PriceParser(MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient) =>
            parser = new()
            {
                { PriceSourceTypes.MOEX, new MoexParser(moexClient) },
                { PriceSourceTypes.Tdameritrade, new TdameritradeParser(tdAmeritradeClient) }
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
