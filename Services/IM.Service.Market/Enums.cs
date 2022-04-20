namespace IM.Service.Market;

public static class Enums
{
    public enum Sources : byte
    {
        Manual = 1,
        Moex = 2,
        Spbex = 3,
        Tdameritrade = 4,
        Investing = 5,
        Yahoo = 6
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