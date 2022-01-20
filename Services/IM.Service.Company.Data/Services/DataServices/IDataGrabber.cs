using IM.Service.Company.Data.Models.Data;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DataServices;

public interface IDataGrabber
{
    Task GrabHistoryDataAsync(string source, DataConfigModel config);
    Task GrabHistoryDataAsync(string source, IEnumerable<DataConfigModel> configs);
           
    Task GrabCurrentDataAsync(string source, DataConfigModel config);
    Task GrabCurrentDataAsync(string source, IEnumerable<DataConfigModel> configs);
}