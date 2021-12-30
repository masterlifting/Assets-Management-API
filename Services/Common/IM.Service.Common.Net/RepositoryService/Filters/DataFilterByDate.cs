using IM.Service.Common.Net.Models.Entity.Interfaces;

using System;
using System.Linq.Expressions;

using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.RepositoryService.Filters.DataFilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public class DataFilterByDate<T> where T : IDateIdentity
{
    private readonly HttpRequestFilterType filterType;
    private readonly FilterDateEqualType equalType;

    public int Year { get; }
    public int Month { get; }
    public int Day { get; }

    protected DataFilterByDate(int year)
    {
        equalType = FilterDateEqualType.Year;
        Year = SetYear(year);
        Month = 1;
        Day = 1;
    }
    protected DataFilterByDate(int year, int month)
    {
        equalType = FilterDateEqualType.YearMonth;
        Year = SetYear(year);
        Month = SetMonth(month, filterType);
        Day = 1;
    }
    protected DataFilterByDate(HttpRequestFilterType filterType, int year, int month, int day)
    {
        equalType = FilterDateEqualType.YearMonthDay;
        this.filterType = filterType;
        Year = SetYear(year);
        Month = SetMonth(month, filterType);
        Day = SetDay(day);
    }

    protected Expression<Func<T, bool>> DateExpression => x => filterType == HttpRequestFilterType.More
       ? x.Date.Year > Year || x.Date.Year == Year && (x.Date.Month == Month && x.Date.Day >= Day || x.Date.Month > Month)
       : equalType == FilterDateEqualType.Year
           ? x.Date.Year == Year
           : equalType == FilterDateEqualType.YearMonth
               ? x.Date.Year == Year && x.Date.Month == Month
               : x.Date.Year == Year && x.Date.Month == Month && x.Date.Day == Day;

    protected Expression<Func<T, bool>> Combine(Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
    {
        var invokedExpression = Expression.Invoke(secondExpression, firstExpression.Parameters);
        var combinedExpression = Expression.AndAlso(firstExpression.Body, invokedExpression);
        return Expression.Lambda<Func<T, bool>>(combinedExpression, firstExpression.Parameters);
    }
}