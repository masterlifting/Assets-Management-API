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
        New = 1,
        Ready = 2,
        Computing = 3,
        Computed = 4,
        NotComputed = 5,
        Error = 6
    }
}