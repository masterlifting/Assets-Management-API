namespace IM.Service.Company.Analyzer;

public static class Enums
{
    public enum CompareType : short
    {
        Asc = 100,
        Desc = -100
    }
    public enum StatusType : byte
    {
        Ready = 1,
        Processing = 2,
        Completed = 3,
        Error = 4
    }
}