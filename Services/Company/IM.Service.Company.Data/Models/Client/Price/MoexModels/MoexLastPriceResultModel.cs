namespace IM.Service.Company.Data.Models.Client.Price.MoexModels;

public class MoexLastPriceResultModel
{
    public MoexLastPriceResultModel(MoexLastPriceData? data) => Data = data;
    public MoexLastPriceData? Data { get; }
}