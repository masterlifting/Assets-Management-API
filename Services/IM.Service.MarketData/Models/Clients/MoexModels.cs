namespace IM.Service.MarketData.Models.Clients;

public record MoexCurrentPriceResultModel(MoexCurrentPriceData Data);
public record MoexCurrentPriceData(Marketdata Marketdata);
public record Marketdata(object[][] Data);

public record MoexHistoryPriceResultModel(MoexHistoryPriceData Data, string Ticker);
public record MoexHistoryPriceData(History History);
public record History(object[]?[] Data);