using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Background.PriceUpdaterBackgroundServices.Interfaces
{
    public interface IClientPriceUpdater
    {
        Task<Price[]> GetLastPricesToAddAsync(IEnumerable<(string ticker, DateTime priceDate)> data);
        Task<Price[]> GetLastPricesToUpdateAsync(IEnumerable<(string ticker, DateTime priceDate)> data);
        Task<Price[]> GetHistoryPricesAsync(IEnumerable<(string ticker, DateTime priceDate)> data);
    }
}
