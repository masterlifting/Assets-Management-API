using IM.Service.Data.Models.Services;

namespace IM.Service.Data.Services.DataFounders;

public interface IDataGrabber
{
    Task GrabHistoryDataAsync(DataConfigModel config);
    Task GrabHistoryDataAsync(IEnumerable<DataConfigModel> configs);
           
    Task GrabCurrentDataAsync(DataConfigModel config);
    Task GrabCurrentDataAsync(IEnumerable<DataConfigModel> configs);
}