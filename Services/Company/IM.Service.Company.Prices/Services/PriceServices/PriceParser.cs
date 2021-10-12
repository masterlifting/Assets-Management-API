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
        private readonly Dictionary<string, IPriceParser> parser;
        public PriceParser(MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient) =>
            parser = new(StringComparer.InvariantCultureIgnoreCase)
            {
                { nameof(PriceSourceTypes.MOEX), new MoexParser(moexClient) },
                { nameof(PriceSourceTypes.Tdameritrade), new TdameritradeParser(tdAmeritradeClient) }
            };

        public async Task<Price[]> LoadLastPricesAsync(string source, PriceIdentity data) =>
            parser.ContainsKey(source)
                ? await parser[source].GetLastPricesAsync(source, data)
                : Array.Empty<Price>();
        public async Task<Price[]> LoadLastPricesAsync(string source, IEnumerable<PriceIdentity> data) =>
            parser.ContainsKey(source)
                ? await parser[source].GetLastPricesAsync(source, data)
                : Array.Empty<Price>();

        public async Task<Price[]> LoadHistoryPricesAsync(string source, PriceIdentity data) =>
            parser.ContainsKey(source)
                ? await parser[source].GetHistoryPricesAsync(source, data)
                : Array.Empty<Price>();
        public async Task<Price[]> LoadHistoryPricesAsync(string source, IEnumerable<PriceIdentity> data) =>
            parser.ContainsKey(source)
                ? await parser[source].GetHistoryPricesAsync(source, data)
                : Array.Empty<Price>();
    }
}
