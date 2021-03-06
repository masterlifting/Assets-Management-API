using System;
using System.Linq.Expressions;
using IM.Service.Shared.Models.Entity.Interfaces;
using static IM.Service.Shared.Enums;
using static IM.Service.Shared.Helpers.LogicHelper.PeriodConfigurator;

namespace IM.Service.Shared.SqlAccess.Filters;

public abstract class FilterByDate<T> : IFilter<T> where T : class, IDateIdentity
{
    private int Year { get; }
    private int Month { get; }
    private int Day { get; }
    public Expression<Func<T, bool>> Expression { get; set; }
    
    protected FilterByDate(CompareType compareType, int year)
    {
        Year = SetYear(year);
        Month = 1;
        Day = 1;

        Expression = x => compareType == CompareType.More
            ? x.Date.Year >= Year
            : x.Date.Year == Year;
    }
    protected FilterByDate(CompareType compareType, int year, int month)
    {
        Year = SetYear(year);
        Month = SetMonth(month);
        Day = 1;

        Expression = x => compareType == CompareType.More
            ? x.Date.Year > Year || x.Date.Year == Year && x.Date.Month >= Month
            : x.Date.Year == Year && x.Date.Month == Month;
    }
    protected FilterByDate(CompareType compareType, int year, int month, int day)
    {
        Year = SetYear(year);
        Month = SetMonth(month, compareType);
        Day = SetDay(day);

        Expression = x => compareType == CompareType.More
            ? x.Date.Year > Year || x.Date.Year == Year && (x.Date.Month == Month && x.Date.Day >= Day || x.Date.Month > Month)
            : x.Date.Year == Year && x.Date.Month == Month && x.Date.Day == Day;
    }
}