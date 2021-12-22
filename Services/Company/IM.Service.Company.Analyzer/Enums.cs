namespace IM.Service.Company.Analyzer;

public static class Enums
{
    public enum EntityTypes : byte
    {
        Price = 1,
        Report = 2,
        Coefficient = 3
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
        Starter = 3,
        Computed = 4,
        NotComputed = 5,
        Error = 6
    }
}