using System.Collections.Generic;

namespace IM.Service.Company.Data.Models.Client.Price.TdAmeritradeModels
{
    public class TdAmeritradeLastPriceResultModel
    {
        public TdAmeritradeLastPriceResultModel(Dictionary<string, TdAmeritradeLastPriceData>? data) => Data = data;
        public Dictionary<string, TdAmeritradeLastPriceData>? Data { get; }
    }
}