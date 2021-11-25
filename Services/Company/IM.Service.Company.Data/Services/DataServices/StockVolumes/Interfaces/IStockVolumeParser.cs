using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;

namespace IM.Service.Company.Data.Services.DataServices.StockVolumes.Interfaces;

public interface IStockVolumeParser
{
    Task<StockVolume[]> GetStockVolumesAsync(string source, DateDataConfigModel config);
    Task<StockVolume[]> GetStockVolumesAsync(string source, IEnumerable<DateDataConfigModel> config);
}