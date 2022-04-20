using IM.Service.Common.Net.Models.Entity.Interfaces;

using System;
using System.Linq.Expressions;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.RepositoryService.Filters.FilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public abstract class FilterByQuarter<T> : IFilter<T> where T : class, IQuarterIdentity
{
    private int Year { get; }
    private byte Quarter { get; }
    public Expression<Func<T, bool>> Expression { get; set; }

    protected FilterByQuarter(CompareType compareType, int year)
    {
        Year = SetYear(year);
        Quarter = 1;
        Expression = x => compareType == CompareType.More
            ? x.Year >= Year
            : x.Year == Year;
    }
    protected FilterByQuarter(CompareType compareType, int year, int quarter)
    {
        Year = SetYear(year);
        Quarter = SetQuarter(quarter);
        
        Expression = x => compareType == CompareType.More
            ? x.Year > Year || x.Year == Year && x.Quarter >= Quarter
            : x.Year == Year && x.Quarter == Quarter;
    }
}