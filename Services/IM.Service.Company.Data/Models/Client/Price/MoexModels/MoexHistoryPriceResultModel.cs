namespace IM.Service.Company.Data.Models.Client.Price.MoexModels;

public record MoexHistoryPriceResultModel(MoexHistoryPriceData Data, string Ticker);
public record MoexHistoryPriceData(History History);
public record History(object[]?[] Data);