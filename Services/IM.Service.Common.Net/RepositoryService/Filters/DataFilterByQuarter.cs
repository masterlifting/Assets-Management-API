using IM.Service.Common.Net.Models.Entity.Interfaces;

using System;
using System.Linq.Expressions;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.RepositoryService.Filters.DataFilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public class DataFilterByQuarter<T> where T : IQuarterIdentity
{
    private readonly HttpRequestFilterType filterType;
    private readonly FilterQuarterEqualType equalType;
    private int Year { get; }
    private byte Quarter { get; }

    protected DataFilterByQuarter(int year)
    {
        equalType = FilterQuarterEqualType.Year;
        Year = SetYear(year);
    }
    protected DataFilterByQuarter(int year, int quarter)
    {
        equalType = FilterQuarterEqualType.YearQuarter;
        Year = SetYear(year);
        Quarter = SetQuarter(quarter);
    }
    protected DataFilterByQuarter(HttpRequestFilterType filterType, int year, int quarter)
    {
        equalType = FilterQuarterEqualType.YearQuarter;
        this.filterType = filterType;
        Year = SetYear(year);
        Quarter = SetQuarter(quarter);
    }

    protected Expression<Func<T, bool>> QuarterExpression => x => filterType == HttpRequestFilterType.More
        ? x.Year > Year || x.Year == Year && x.Quarter >= Quarter
        : equalType == FilterQuarterEqualType.Year
            ? x.Year == Year
            : x.Year == Year && x.Quarter == Quarter;
    
    protected Expression<Func<T, bool>> Combine(Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
    {
        var invokedExpression = Expression.Invoke(secondExpression, firstExpression.Parameters);
        var combinedExpression = Expression.AndAlso(firstExpression.Body, invokedExpression);
        return Expression.Lambda<Func<T, bool>>(combinedExpression, firstExpression.Parameters);
    }
}