using IM.Service.Company.Data.Clients.Report;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Models.Data;
using IM.Service.Company.Data.Services.DataServices.StockVolumes.Implementations;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static IM.Service.Company.Data.Enums;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes;

public class StockVolumeGrabber
{
    private readonly Dictionary<string, IDataGrabber> grabber;
    public StockVolumeGrabber(Repository<StockVolume> repository, ILogger<StockVolumeGrabber> logger, InvestingClient investingClient) =>
        grabber = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { nameof(SourceTypes.Investing), new InvestingGrabber(repository, logger, investingClient) }
        };

    public bool IsSource(string source) => grabber.ContainsKey(source);

    public Task GrabCurrentStockVolumesAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabCurrentStockVolumesAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, configs)
            : Task.CompletedTask;

    public Task GrabHistoryStockVolumesAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabHistoryStockVolumesAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, configs)
            : Task.CompletedTask;
}