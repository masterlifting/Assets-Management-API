namespace IM.Services.Companies.Prices.Api.Models.Client.TdAmeritradeModels
{
    public class TdAmeritradeHistoryPriceResultModel
    {
        public TdAmeritradeHistoryPriceResultModel(TdAmeritradeHistoryPriceData data, string ticker)
        {
            Data = data;
            Ticker = ticker;
        }

        public TdAmeritradeHistoryPriceData Data { get; }
        public string Ticker { get; }
    }
}