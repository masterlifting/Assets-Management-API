using IM.Service.Company.Data.Models.Data;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices.StockSplits.Implementations;

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