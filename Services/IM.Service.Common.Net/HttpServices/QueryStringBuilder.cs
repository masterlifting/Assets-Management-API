using System.Collections.Generic;
using static IM.Service.Common.Net.RepositoryService.Filters.DataFilterSetter;
using static IM.Service.Common.Net.Enums;

namespace IM.Service.Common.Net.HttpServices;

public static class QueryStringBuilder
{
    public static string GetQueryString(HttpRequestFilterType filter, int year)
    {
        year = SetYear(year);

        return filter == HttpRequestFilterType.More
            ? $"?year={year}"
            : $"/{year}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year)
    {
        year = SetYear(year);

        return filter == HttpRequestFilterType.More
            ? $"/{companyId}?year={year}"
            : $"/{companyId}/{year}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, IEnumerable<string> companyIds, int year)
    {
        year = SetYear(year);

        return filter == HttpRequestFilterType.More
            ? $"/{string.Join(",", companyIds)}?year={year}"
            : $"/{string.Join(",", companyIds)}/{year}";
    }

    public static string GetQueryString(HttpRequestFilterType filter, int year, int month)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);

        return filter == HttpRequestFilterType.More
            ? $"?year={year}&month={month}"
            : $"/{year}/{month}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year, int month)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);

        return filter == HttpRequestFilterType.More
            ? $"/{companyId}?year={year}&month={month}"
            : $"/{companyId}/{year}/{month}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, IEnumerable<string> companyIds, int year, int month)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);

        return filter == HttpRequestFilterType.More
            ? $"/{string.Join(",", companyIds)}?year={year}&month={month}"
            : $"/{string.Join(",", companyIds)}/{year}/{month}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, int year, int month, int day)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);
        day = SetDay(day);

        return filter == HttpRequestFilterType.More
            ? $"?year={year}&month={month}&day={day}"
            : $"/{year}/{month}/{day}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year, int month, int day)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);
        day = SetDay(day);

        return filter == HttpRequestFilterType.More
            ? $"/{companyId}?year={year}&month={month}&day={day}"
            : $"/{companyId}/{year}/{month}/{day}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, IEnumerable<string> companyIds, int year, int month, int day)
    {
        year = SetYear(year);
        month = SetMonth(month, filter);
        day = SetDay(day);

        return filter == HttpRequestFilterType.More
            ? $"/{string.Join(",",companyIds)}?year={year}&month={month}&day={day}"
            : $"/{string.Join(",", companyIds)}/{year}/{month}/{day}";
    }

    public static string GetQueryString(HttpRequestFilterType filter, int year, byte quarter)
    {
        year = SetYear(year);
        quarter = SetQuarter(quarter);

        return filter == HttpRequestFilterType.More
            ? $"?year={year}&quarter={quarter}"
            : $"/{year}/{quarter}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, string companyId, int year, byte quarter)
    {
        year = SetYear(year);
        quarter = SetQuarter(quarter);

        return filter == HttpRequestFilterType.More
            ? $"/{companyId}?year={year}&quarter={quarter}"
            : $"/{companyId}/{year}/{quarter}";
    }
    public static string GetQueryString(HttpRequestFilterType filter, IEnumerable<string> companyIds, int year, byte quarter)
    {
        year = SetYear(year);
        quarter = SetQuarter(quarter);

        return filter == HttpRequestFilterType.More
            ? $"/{string.Join(",", companyIds)}?year={year}&quarter={quarter}"
            : $"/{string.Join(",", companyIds)}/{year}/{quarter}";
    }
}