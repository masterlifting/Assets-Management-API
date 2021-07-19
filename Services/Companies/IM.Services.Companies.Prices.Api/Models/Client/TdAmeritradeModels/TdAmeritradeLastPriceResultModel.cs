using System.Collections.Generic;

namespace IM.Services.Companies.Prices.Api.Models.Client.TdAmeritradeModels
{
    public class TdAmeritradeLastPriceResultModel
    {
        public TdAmeritradeLastPriceResultModel(Dictionary<string, TdAmeritradeLastPriceData> data) => Data = data;
        public Dictionary<string, TdAmeritradeLastPriceData> Data { get; }
    }
}