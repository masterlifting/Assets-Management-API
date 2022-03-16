using System;
using System.Linq;
using System.Linq.Expressions;

using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helper.ExpressionHelper;


namespace IM.Service.Common.Net.RepositoryService.Filters;

public class CompanyDataFilterByQuarter<T> : FilterByQuarter<T> where T : class, ICompanyQuarterIdentity
{
    public Expression<Func<T, bool>> FilterExpression { get; }

    public CompanyDataFilterByQuarter(int year) : base(year) => FilterExpression = Expression;
    public CompanyDataFilterByQuarter(HttpRequestFilterType filterType, int year, int quarter) : base(filterType, year, quarter) => FilterExpression = Expression;

    public CompanyDataFilterByQuarter(string companyId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        FilterExpression = Combine(x => companyId == x.CompanyId, Expression);
    }
   
    public CompanyDataFilterByQuarter(HttpRequestFilterType filterType, string companyId, int year, int quarter) : base(filterType, year, quarter)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        FilterExpression = Combine(x => companyId == x.CompanyId, Expression);
    }

    public CompanyDataFilterByQuarter(string[] companyIds, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    
    public CompanyDataFilterByQuarter(HttpRequestFilterType filterType, string[] companyIds, int year, int quarter) : base(filterType, year, quarter)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
}