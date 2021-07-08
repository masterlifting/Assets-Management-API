using IM.Services.Companies.Prices.Api.Clients.Models.MoexModels;
using IM.Services.Companies.Prices.Api.Clients.Models.TdAmeritradeModels;
using IM.Services.Companies.Prices.Api.DataAccess.Entities;
using System.Collections.Generic;

namespace IM.Services.Companies.Prices.Api.Services.Mapper
{
    public interface IPriceMapper
    {
        Price[] MapToPrices(MoexLastPriceResultModel clientResult, IEnumerable<string> tickers);
        Price[] MapToPrices(MoexHistoryPriceResultModel clientResult);
        Price[] MapToPrices(TdAmeritradeLastPriceResultModel clientResult);
        Price[] MapToPrices(TdAmeritradeHistoryPriceResultModel clientResult);
    }
}