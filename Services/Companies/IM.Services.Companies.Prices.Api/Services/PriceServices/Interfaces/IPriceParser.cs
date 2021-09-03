using CommonServices.Models.Entity;

using IM.Services.Companies.Prices.Api.DataAccess.Entities;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.PriceServices.Interfaces
{
    public interface IPriceParser
    {
        Task<Price[]> GetLastPricesToAddAsync(IEnumerable<PriceIdentity> prices);
        Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<PriceIdentity> prices);
        Task<Price[]> GetHistoryPricesAsync(IEnumerable<PriceIdentity> prices);
    }
}
