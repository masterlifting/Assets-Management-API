using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;
using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helper.ExpressionHelper;


namespace IM.Service.Market.Domain.DataAccess.Filters;

public class CompanyDateFilter<T> : FilterByDate<T> where T : class, IDateIdentity, ICompanyIdentity
{
    public string[] CompanyIds { get; } = Array.Empty<string>();
    public string? CompanyId { get; }

    public CompanyDateFilter(int year) : base(year) { }
    public CompanyDateFilter(int year, int month) : base(year, month) { }
    public CompanyDateFilter(HttpRequestFilterType filterType, int year, int month, int day) : base(filterType, year, month, day) { }

    public CompanyDateFilter(string companyId)
    {
        CompanyId = companyId;
        Expression = x => companyId == x.CompanyId;
    }
    public CompanyDateFilter(string companyId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public CompanyDateFilter(string companyId, int year, int month) : base(year, month)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public CompanyDateFilter(HttpRequestFilterType filterType, string companyId, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }

    public CompanyDateFilter(string[] companyIds)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = x => companyIds.Contains(x.CompanyId);
    }
    public CompanyDateFilter(string[] companyIds, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public CompanyDateFilter(string[] companyIds, int year, int month) : base(year, month)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public CompanyDateFilter(HttpRequestFilterType filterType, string[] companyIds, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
}