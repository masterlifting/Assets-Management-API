using CommonServices.Models.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;
using IM.Service.Company.Prices.DataAccess.Entities;


namespace IM.Service.Company.Prices.Services.PriceServices.Interfaces
{
    public interface IPriceParser
    {
        Task<Price[]> GetLastPricesToAddAsync(IEnumerable<PriceIdentity> prices);
        Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<PriceIdentity> prices);
        Task<Price[]> GetHistoryPricesAsync(IEnumerable<PriceIdentity> prices);
    }
}
