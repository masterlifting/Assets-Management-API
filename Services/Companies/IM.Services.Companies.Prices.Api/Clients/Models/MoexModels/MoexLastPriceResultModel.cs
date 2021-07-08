namespace IM.Services.Companies.Prices.Api.Clients.Models.MoexModels
{
    public class MoexLastPriceResultModel
    {
        public MoexLastPriceResultModel(MoexLastPriceData data) => Data = data;
        public MoexLastPriceData Data { get; }
    }
}