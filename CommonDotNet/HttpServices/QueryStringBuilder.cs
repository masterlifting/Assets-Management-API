
using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryDateSetter;

namespace CommonServices.HttpServices
{
    public static class QueryStringBuilder
    {
        public static string GetQueryString(int year) => $"/{SetYear(year)}";
        public static string GetQueryString(string ticker, int year) => $"/{ticker}/{SetYear(year)}";
        public static string GetQueryString(HttpRequestFilterType filter, int year, int month) => $"/{SetYear(year)}/{SetMonth(month, filter)}";
        public static string GetQueryString(HttpRequestFilterType filter, string ticker, int year, int month) => $"/{ticker}/{SetYear(year)}/{SetMonth(month, filter)}";
        public static string GetQueryString(HttpRequestFilterType filter, int year, int month, int day)
        {
            year = SetYear(year);
            month = SetMonth(month, filter);
            day = SetDay(day);

            return filter == HttpRequestFilterType.More
                ? $"?year={year}&month={month}&day={day}"
                : $"/{year}/{month}/{day}";
        }
        public static string GetQueryString(HttpRequestFilterType filter, string ticker, int year, int month, int day)
        {
            year = SetYear(year);
            month = SetMonth(month, filter);
            day = SetDay(day);

            return filter == HttpRequestFilterType.More
                ? $"/{ticker}?year={year}&month={month}&day={day}"
                : $"/{ticker}/{year}/{month}/{day}";
        }
        public static string GetQueryString(int year, byte quarter) => $"/{SetYear(year)}/{SetQuarter(quarter)}";
        public static string GetQueryString(string ticker, int year, byte quarter) => $"/{ticker}/{SetYear(year)}/{SetQuarter(quarter)}";
        public static string GetQueryString(HttpRequestFilterType filter, int year, byte quarter)
        {
            year = SetYear(year);
            quarter = SetQuarter(quarter);

            return filter == HttpRequestFilterType.More
                ? $"?year={year}&quarter={quarter}"
                : $"/{year}/{quarter}";
        }
        public static string GetQueryString(HttpRequestFilterType filter, string ticker, int year, byte quarter)
        {
            year = SetYear(year);
            quarter = SetQuarter(quarter);

            return filter == HttpRequestFilterType.More
                ? $"/{ticker}?year={year}&quarter={quarter}"
                : $"/{ticker}/{year}/{quarter}";
        }
    }
}
