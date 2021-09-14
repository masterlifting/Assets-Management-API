namespace IM.Service.Company.Analyzer
{
    public static class Enums
    {
        public enum CompareType : short
        {
            Asc = 100,
            Desc = -100
        }
        public enum StatusType : byte
        {
            ToCalculate = 1,
            Calculating = 2,
            CalculatedPartial = 3,
            Calculated = 4,
            Error = 5
        }
    }
}
