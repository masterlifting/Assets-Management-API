using IM.Service.Company.Data.Clients.Price;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.Prices.Implementations;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.Prices;

public class PriceGrabber
{
    private readonly Dictionary<string, IDataGrabber> grabber;
    public PriceGrabber(Repository<Price> repository, ILogger<PriceGrabber> logger, MoexClient moexClient, TdAmeritradeClient tdAmeritradeClient) =>
        grabber = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(Enums.SourceTypes.Moex), new MoexGrabber(repository, logger, moexClient) },
            { nameof(Enums.SourceTypes.Tdameritrade), new TdameritradeGrabber(repository, logger, tdAmeritradeClient) }
        };

    public bool IsSource(string source) => grabber.ContainsKey(source);

    public Task GrabCurrentPricesAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabCurrentPricesAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, configs)
            : Task.CompletedTask;

    public Task GrabHistoryPricesAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabHistoryPricesAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, configs)
            : Task.CompletedTask;
}