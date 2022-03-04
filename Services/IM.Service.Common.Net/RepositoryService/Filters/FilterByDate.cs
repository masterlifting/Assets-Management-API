using IM.Service.Common.Net.Models.Entity.Interfaces;

using System;
using System.Linq.Expressions;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.RepositoryService.Filters.FilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public abstract class FilterByDate<T> : IFilter<T> where T : class, IDateIdentity
{
    public int Year { get; }
    public int Month { get; }
    public int Day { get; }
    public Expression<Func<T, bool>> Expression { get; set; }

    public FilterByDate()
    {
        Expression = x => true;
    }
    public FilterByDate(int year)
    {
        Year = SetYear(year);
        Month = 1;
        Day = 1;

        Expression = x => x.Date.Year == Year;
    }
    public FilterByDate(int year, int month)
    {
        Year = SetYear(year);
        Month = SetMonth(month);
        Day = 1;

        Expression = x => x.Date.Year == Year && x.Date.Month == Month;
    }
    public FilterByDate(HttpRequestFilterType filterType, int year, int month, int day)
    {
        Year = SetYear(year);
        Month = SetMonth(month, filterType);
        Day = SetDay(day);

        Expression = x => filterType == HttpRequestFilterType.More
            ? x.Date.Year > Year || x.Date.Year == Year && (x.Date.Month == Month && x.Date.Day >= Day || x.Date.Month > Month)
            : x.Date.Year == Year && x.Date.Month == Month && x.Date.Day == Day;
    }
}