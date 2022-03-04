namespace IM.Service.Data;

public static class Enums
{
    public enum Sources : byte
    {
        Official = 1,
        Moex = 2,
        Tdameritrade = 3,
        Investing = 4
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