using System;
using System.Linq;
using System.Linq.Expressions;

using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

using static IM.Service.Common.Net.Enums;


namespace IM.Service.Common.Net.RepositoryService.Filters;

public class CompanyDataFilterByDate<T> : FilterByDate<T> where T : class, ICompanyDateIdentity
{
    public Expression<Func<T, bool>> FilterExpression { get; }

    public string[] CompanyIds { get; } = Array.Empty<string>();
    public string? CompanyId { get; }

    public CompanyDataFilterByDate(int year) : base(year) => FilterExpression = Expression;
    public CompanyDataFilterByDate(int year, int month) : base(year, month) => FilterExpression = Expression;
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, int year, int month, int day) : base(filterType, year, month, day) => FilterExpression = Expression;

    public CompanyDataFilterByDate(string companyId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        FilterExpression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public CompanyDataFilterByDate(string companyId, int year, int month) : base(year, month)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        FilterExpression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, string companyId, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        FilterExpression = Combine(x => companyId == x.CompanyId, Expression);
    }

    public CompanyDataFilterByDate(string[] companyIds, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public CompanyDataFilterByDate(string[] companyIds, int year, int month) : base(year, month)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, string[] companyIds, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
}