using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Market.Models.Data;

namespace IM.Service.Market.Services.DataServices;

public interface IDataGrabber
{
    Task GrabHistoryDataAsync(string source, DataConfigModel config);
    Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs);
           
    Task GrabCurrentDataAsync(string source, DataConfigModel config);
    Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs);
}