using CommonServices.Models.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Prices.DataAccess.Entities;


namespace IM.Service.Company.Prices.Services.PriceServices.Interfaces
{
    public interface IPriceParser
    {
        Task<Price[]> GetHistoryPricesAsync(string source, PriceIdentity data);
        Task<Price[]> GetHistoryPricesAsync(string source, IEnumerable<PriceIdentity> data);
        
        Task<Price[]> GetLastPricesAsync(string source, PriceIdentity data);
        Task<Price[]> GetLastPricesAsync(string source, IEnumerable<PriceIdentity> data);
    }
}
