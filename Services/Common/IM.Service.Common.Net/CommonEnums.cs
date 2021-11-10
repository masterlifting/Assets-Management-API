namespace IM.Service.Common.Net
{
    public static class CommonEnums
    {
        public enum HttpRequestFilterType : byte
        {
            Equal = 1,
            More = 2
        }
        public enum FilterDateEqualType
        {
            Year,
            YearMonth,
            YearMonthDay
        }
        public enum FilterQuarterEqualType
        {
            Year,
            YearQuarter
        }
    }
}
