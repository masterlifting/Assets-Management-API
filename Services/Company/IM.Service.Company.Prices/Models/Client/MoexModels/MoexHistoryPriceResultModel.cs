namespace IM.Service.Company.Prices.Models.Client.MoexModels
{
    public class MoexHistoryPriceResultModel
    {
        public MoexHistoryPriceResultModel(MoexHistoryPriceData? data, string ticker)
        {
            Data = data;
            Ticker = ticker;
        }

        public MoexHistoryPriceData? Data { get; }
        public string Ticker { get; }
    }
}