using System;

using static IM.Service.Common.Net.Enums;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public static class DataFilterSetter
{
    public static int SetDay(int day) => day > 31 ? 31 : day <= 0 ? 1 : day;
    public static int SetMonth(int month, HttpRequestFilterType filter)
    {
        var targetMonth = filter == HttpRequestFilterType.More && DateTime.UtcNow.Day == 1
            ? DateTime.UtcNow.AddMonths(-1).Month
            : month;
        return targetMonth > 12 ? 12 : targetMonth <= 0 ? 1 : targetMonth;
    }
    public static int SetYear(int year) => year > DateTime.UtcNow.Year ? DateTime.UtcNow.Year : year <= 1985 ? DateTime.UtcNow.Year : year;
    public static byte SetQuarter(int quarter) => quarter > 4 ? (byte)4 : quarter <= 0 ? (byte)1 : (byte)quarter;
}