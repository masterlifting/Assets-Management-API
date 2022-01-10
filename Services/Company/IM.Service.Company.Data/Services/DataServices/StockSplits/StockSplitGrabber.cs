using IM.Service.Company.Data.Models.Data;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.StockSplits;

public class StockSplitGrabber
{
    private readonly Dictionary<string, IDataGrabber> grabber;
    public StockSplitGrabber() => grabber = new(StringComparer.InvariantCultureIgnoreCase);

    public bool IsSource(string source) => grabber.ContainsKey(source);

    public Task GrabCurrentStockSplitsAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabCurrentStockSplitsAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, configs)
            : Task.CompletedTask;

    public Task GrabHistoryStockSplitsAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabHistoryStockSplitsAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, configs)
            : Task.CompletedTask;
}