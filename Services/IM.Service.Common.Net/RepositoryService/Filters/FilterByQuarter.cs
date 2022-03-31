using IM.Service.Common.Net.Models.Entity.Interfaces;

using System;
using System.Linq.Expressions;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.RepositoryService.Filters.FilterSetter;

namespace IM.Service.Common.Net.RepositoryService.Filters;

public abstract class FilterByQuarter<T> : IFilter<T> where T : class, IQuarterIdentity
{
    public int Year { get; }
    public byte Quarter { get; }
    public Expression<Func<T, bool>> Expression { get; set; }

    protected FilterByQuarter()
    {
        Expression = x => true;
    }
    protected FilterByQuarter(int year)
    {
        Year = SetYear(year);

        Expression = x => x.Year == Year;
    }
    protected FilterByQuarter(HttpRequestFilterType filterType, int year, int quarter)
    {
        Year = SetYear(year);
        Quarter = SetQuarter(quarter);
        
        Expression = x => filterType == HttpRequestFilterType.More
            ? x.Year > Year || x.Year == Year && x.Quarter >= Quarter
            : x.Year == Year && x.Quarter == Quarter;
    }
}