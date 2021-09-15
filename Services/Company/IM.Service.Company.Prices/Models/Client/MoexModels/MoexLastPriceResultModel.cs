namespace IM.Service.Company.Prices.Models.Client.MoexModels
{
    public class MoexLastPriceResultModel
    {
        public MoexLastPriceResultModel(MoexLastPriceData? data) => Data = data;
        public MoexLastPriceData? Data { get; }
    }
}