namespace IM.Service.Company.Data.Models.Client.Price.MoexModels;

public record MoexCurrentPriceResultModel(MoexCurrentPriceData Data);
public record MoexCurrentPriceData(Marketdata Marketdata);
public record Marketdata(object[][] Data);