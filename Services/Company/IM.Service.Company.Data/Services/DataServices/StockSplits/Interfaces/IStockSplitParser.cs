using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;

namespace IM.Service.Company.Data.Services.DataServices.StockSplits.Interfaces;

public interface IStockSplitParser
{
    Task<StockSplit[]> GetStockSplitsAsync(string source, DateDataConfigModel config);
    Task<StockSplit[]> GetStockSplitsAsync(string source, IEnumerable<DateDataConfigModel> config);
}