using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Market.Models.Data;

namespace IM.Service.Market.Services.DataServices.StockSplits.Implementations;

public class NotImplementedGrabber : IDataGrabber
{
    public Task GrabCurrentDataAsync(string source, DataConfigModel config)
    {
        throw new NotImplementedException();
    }
    public Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs)
    {
        throw new NotImplementedException();
    }

    public Task GrabHistoryDataAsync(string source, DataConfigModel config)
    {
        throw new NotImplementedException();
    }
    public Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs)
    {
        throw new NotImplementedException();
    }
}