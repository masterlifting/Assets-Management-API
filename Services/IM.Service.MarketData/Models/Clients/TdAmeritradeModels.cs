namespace IM.Service.MarketData.Models.Clients;

public record Candles(long datetime, decimal high);
public record TdAmeritradeHistoryPriceData(Candles[]? candles);
public record TdAmeritradeLastPriceData(long regularMarketTradeTimeInLong, decimal lastPrice);
public record TdAmeritradeHistoryPriceResultModel(TdAmeritradeHistoryPriceData? data, string ticker);
public record TdAmeritradeLastPriceResultModel(Dictionary<string, TdAmeritradeLastPriceData>? data);