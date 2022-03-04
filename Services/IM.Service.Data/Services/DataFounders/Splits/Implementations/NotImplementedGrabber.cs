using IM.Service.Data.Models.Services;

namespace IM.Service.Data.Services.DataFounders.Splits.Implementations;

public class NotImplementedGrabber : IDataGrabber
{
    public Task GrabCurrentDataAsync(DataConfigModel config)
    {
        throw new NotImplementedException();
    }
    public Task GrabCurrentDataAsync(IEnumerable<DataConfigModel> configs)
    {
        throw new NotImplementedException();
    }

    public Task GrabHistoryDataAsync(DataConfigModel config)
    {
        throw new NotImplementedException();
    }
    public Task GrabHistoryDataAsync(IEnumerable<DataConfigModel> configs)
    {
        throw new NotImplementedException();
    }
}