using IM.Service.Data.Models.Services;

namespace IM.Service.Data.Services.DataFounders;

public abstract class DataGrabber
{
    private readonly Dictionary<string, IDataGrabber> grabber;
    protected DataGrabber(Dictionary<string, IDataGrabber> grabber) => this.grabber = grabber;

    public bool IsSource(string source) => grabber.ContainsKey(source);

    public Task GrabCurrentDataAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(config)
            : Task.CompletedTask;
    public Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabCurrentDataAsync(configs)
            : Task.CompletedTask;

    public Task GrabHistoryDataAsync(string source, DataConfigModel config) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(config)
            : Task.CompletedTask;
    public Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs) =>
        grabber.ContainsKey(source)
            ? grabber[source].GrabHistoryDataAsync(configs)
            : Task.CompletedTask;
}