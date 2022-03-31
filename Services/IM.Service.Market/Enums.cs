namespace IM.Service.Market;

public static class Enums
{
    public enum Sources : byte
    {
        Moex = 1,
        Spbex = 2,
        Tdameritrade = 3,
        Investing = 4,
        Yahoo = 5
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