namespace IM.Services.Companies.Prices.Api.Models.Client.MoexModels
{
    public class MoexLastPriceResultModel
    {
        public MoexLastPriceResultModel(MoexLastPriceData data) => Data = data;
        public MoexLastPriceData Data { get; }
    }
}