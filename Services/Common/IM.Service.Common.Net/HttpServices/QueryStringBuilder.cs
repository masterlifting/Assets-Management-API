using static IM.Service.Common.Net.RepositoryService.Filters.DataFilterSetter;
using static IM.Service.Common.Net.CommonEnums;

namespace IM.Service.Common.Net.HttpServices;

public static class QueryStringBuilder
{
    public static string GetQueryString(int year) => $"/{SetYear(year)}";
    public static string GetQueryString(string companyId, int year) => $"/{companyId}/{SetYear(year)}";
    public static string GetQueryString(HttpRequestFilterType filter, int year, int month) => $"/{SetYear(year)}/{SetMonth(month, filter)}";
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year, int month) => $"/{companyId}/{SetYear(year)}/{SetMonth(month, filter)}";
    public static string GetQueryString(HttpRequestFilterType filter, int year, int month, int day)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);
        day = SetDay(day);

        return filter == HttpRequestFilterType.More
            ? $"?Year={year}&Month={month}&Day={day}"
            : $"/{year}/{month}/{day}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year, int month, int day)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);
        day = SetDay(day);

        return filter == HttpRequestFilterType.More
            ? $"/{companyId}?Year={year}&Month={month}&Day={day}"
            : $"/{companyId}/{year}/{month}/{day}";
    }
    public static string GetQueryString(int year, byte quarter) => $"/{SetYear(year)}/{SetQuarter(quarter)}";
    public static string GetQueryString(string companyId, int year, byte quarter) => $"/{companyId}/{SetYear(year)}/{SetQuarter(quarter)}";
    public static string GetQueryString(HttpRequestFilterType filter, int year, byte quarter)
    {
        year = SetYear(year);
        quarter = SetQuarter(quarter);

        return filter == HttpRequestFilterType.More
            ? $"?Year={year}&Quarter={quarter}"
            : $"/{year}/{quarter}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year, byte quarter)
    {
        year = SetYear(year);
        quarter = SetQuarter(quarter);

        return filter == HttpRequestFilterType.More
            ? $"/{companyId}?Year={year}&Quarter={quarter}"
            : $"/{companyId}/{year}/{quarter}";
    }
}