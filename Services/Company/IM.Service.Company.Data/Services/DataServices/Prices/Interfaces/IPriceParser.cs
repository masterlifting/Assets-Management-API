using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.Models.Data;


namespace IM.Service.Company.Data.Services.DataServices.Prices.Interfaces
{
    public interface IPriceParser
    {
        Task<Price[]> GetHistoryPricesAsync(string source, DateDataConfigModel config);
        Task<Price[]> GetHistoryPricesAsync(string source, IEnumerable<DateDataConfigModel> config);
        
        Task<Price[]> GetLastPricesAsync(string source, DateDataConfigModel config);
        Task<Price[]> GetLastPricesAsync(string source, IEnumerable<DateDataConfigModel> config);
    }
}
