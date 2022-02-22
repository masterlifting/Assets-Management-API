using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Market.Models.Data;

namespace IM.Service.Market.Services.DataServices;

public abstract class DataGrabber
{
    private readonly Dictionary<string, IDataGrabber> grabber;
    protected DataGrabber(Dictionary<string, IDataGrabber> grabber) => this.grabber = grabber;

    public bool IsSource(string source) => grabber.ContainsKey(source);

    public Task GrabCurrentDataAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(source, configs)
            : Task.CompletedTask;

    public Task GrabHistoryDataAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, config)
            : Task.CompletedTask;
    public Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(source, configs)
            : Task.CompletedTask;
}