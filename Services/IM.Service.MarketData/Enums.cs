namespace IM.Service.MarketData;

public static class Enums
{
    public enum Sources : byte
    {
        Manual = 1,
        Official = 2,
        Moex = 3,
        Tdameritrade = 4,
        Investing = 5
    }
    public enum CompareTypes : short
    {
        Asc = 100,
        Desc = -100
    }
    public enum Statuses : byte
    {
        Ready = 1,
        Processing = 2,
        Computed = 3,
        NotComputed = 4,
        Error = 5
    }
}