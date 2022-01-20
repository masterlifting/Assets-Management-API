namespace IM.Service.Company.Data.Models.Client.Price.TdAmeritradeModels
{
    public class TdAmeritradeHistoryPriceResultModel
    {
        public TdAmeritradeHistoryPriceResultModel(TdAmeritradeHistoryPriceData? data, string ticker)
        {
            Data = data;
            Ticker = ticker;
        }

        public TdAmeritradeHistoryPriceData? Data { get; }
        public string Ticker { get; }
    }
}