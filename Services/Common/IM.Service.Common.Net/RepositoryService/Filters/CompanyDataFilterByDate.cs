using System;
using System.Linq;
using System.Linq.Expressions;

using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;

using static IM.Service.Common.Net.CommonEnums;


namespace IM.Service.Common.Net.RepositoryService.Filters;

public class CompanyDataFilterByDate<T> : DataFilterByDate<T> where T : class, ICompanyDateIdentity
{
    public Expression<Func<T, bool>> FilterExpression { get; }

    public string[] CompanyIds { get; } = Array.Empty<string>();
    public string? CompanyId { get; }

    public CompanyDataFilterByDate(int year) : base(year) => FilterExpression = DateExpression;
    public CompanyDataFilterByDate(int year, int month) : base(year, month) => FilterExpression = DateExpression;
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, int year, int month, int day) : base(filterType, year, month, day) => FilterExpression = DateExpression;

    public CompanyDataFilterByDate(string companyId, int year) : base(year)
    {
        companyId = companyId.ToUpperInvariant().Trim();
        CompanyId = companyId;
        FilterExpression = Combine(x => companyId == x.CompanyId, DateExpression);
    }
    public CompanyDataFilterByDate(string companyId, int year, int month) : base(year, month)
    {
        companyId = companyId.ToUpperInvariant().Trim();
        CompanyId = companyId;
        FilterExpression = Combine(x => companyId == x.CompanyId, DateExpression);
    }
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, string companyId, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyId = companyId.ToUpperInvariant().Trim();
        CompanyId = companyId;
        FilterExpression = Combine(x => companyId == x.CompanyId, DateExpression);
    }

    public CompanyDataFilterByDate(string[] companyIds, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), DateExpression);
    }
    public CompanyDataFilterByDate(string[] companyIds, int year, int month) : base(year, month)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), DateExpression);
    }
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, string[] companyIds, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        FilterExpression = Combine(x => companyIds.Contains(x.CompanyId), DateExpression);
    }
}