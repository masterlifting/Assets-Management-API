using IM.Service.Company.Data.Clients.Price;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Prices.Implementations;
using IM.Service.Company.Data.Services.DataServices.Prices.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.Prices
{
    public class PriceParser
    {
        private readonly Dictionary<string, IPriceParser> parser;
        public PriceParser(MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient) =>
            parser = new(StringComparer.InvariantCultureIgnoreCase)
            {
                { nameof(Enums.SourceTypes.Moex), new MoexParser(moexClient) },
                { nameof(Enums.SourceTypes.Tdameritrade), new TdameritradeParser(tdAmeritradeClient) }
            };

        public async Task<Price[]> LoadLastPricesAsync(string source, PriceDataConfigModel config) =>
            parser.ContainsKey(source)
                ? await parser[source].GetLastPricesAsync(source, config)
                : Array.Empty<Price>();
        public async Task<Price[]> LoadLastPricesAsync(string source, IEnumerable<PriceDataConfigModel> config) =>
            parser.ContainsKey(source)
                ? await parser[source].GetLastPricesAsync(source, config)
                : Array.Empty<Price>();

        public async Task<Price[]> LoadHistoryPricesAsync(string source, PriceDataConfigModel config) =>
            parser.ContainsKey(source)
                ? await parser[source].GetHistoryPricesAsync(source, config)
                : Array.Empty<Price>();
        public async Task<Price[]> LoadHistoryPricesAsync(string source, IEnumerable<PriceDataConfigModel> config) =>
            parser.ContainsKey(source)
                ? await parser[source].GetHistoryPricesAsync(source, config)
                : Array.Empty<Price>();
    }
}
