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

    public CompanyDateFilter(CompareType compareType, int year) : base(compareType, year) { }
    public CompanyDateFilter(CompareType compareType, int year, int month) : base(compareType, year, month) { }
    public CompanyDateFilter(CompareType compareType, int year, int month, int day) : base(compareType, year, month, day) { }

    public CompanyDateFilter(CompareType compareType, string companyId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }
    public CompanyDateFilter(CompareType compareType, string companyId, int year, int month) : base(compareType, year, month)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }
    public CompanyDateFilter(CompareType compareType, string companyId, int year, int month, int day) : base(compareType, year, month, day)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }

    public CompanyDateFilter(CompareType compareType, IEnumerable<string> companyIds, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    public CompanyDateFilter(CompareType compareType, IEnumerable<string> companyIds, int year, int month) : base(compareType, year, month)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    public CompanyDateFilter(CompareType compareType, IEnumerable<string> companyIds, int year, int month, int day) : base(compareType, year, month, day)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
}