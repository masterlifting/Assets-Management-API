using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helper.ExpressionHelper;

namespace IM.Service.MarketData.Domain.DataAccess.Filters;

public class DateFilter<T> : FilterByDate<T> where T : class, IDateIdentity, IDataIdentity
{
    public string[] CompanyIds { get; } = Array.Empty<string>();
    public string? CompanyId { get; }
    public byte? SourceId { get; }

    public DateFilter(int year) : base(year) { }
    public DateFilter(int year, int month) : base(year, month) { }
    public DateFilter(HttpRequestFilterType filterType, int year, int month, int day) : base(filterType, year, month, day) { }

    public DateFilter(string companyId)
    {
        CompanyId = companyId;
        Expression = x => companyId == x.CompanyId;
    }
    public DateFilter(string companyId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public DateFilter(string companyId, int year, int month) : base(year, month)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public DateFilter(HttpRequestFilterType filterType, string companyId, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }

    public DateFilter(byte sourceId)
    {
        SourceId = sourceId;
        Expression = x => sourceId == x.SourceId;
    }
    public DateFilter(byte sourceId, int year) : base(year)
    {
        SourceId = sourceId;
        Expression = Combine(x => sourceId == x.SourceId, Expression);
    }
    public DateFilter(byte sourceId, int year, int month) : base(year, month)
    {
        SourceId = sourceId;
        Expression = Combine(x => sourceId == x.SourceId, Expression);
    }
    public DateFilter(HttpRequestFilterType filterType, byte sourceId, int year, int month, int day) : base(filterType, year, month, day)
    {
        SourceId = sourceId;
        Expression = Combine(x => sourceId == x.SourceId, Expression);
    }

    public DateFilter(string companyId, byte sourceId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = x => companyId == x.CompanyId && sourceId == x.SourceId;
    }
    public DateFilter(string companyId, byte sourceId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        SourceId = sourceId;
        Expression = Combine(x => companyId == x.CompanyId && sourceId == x.SourceId, Expression);
    }
    public DateFilter(string companyId, byte sourceId, int year, int month) : base(year, month)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        SourceId = sourceId;
        Expression = Combine(x => companyId == x.CompanyId && sourceId == x.SourceId, Expression);
    }
    public DateFilter(HttpRequestFilterType filterType, string companyId, byte sourceId, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        SourceId = sourceId;
        Expression = Combine(x => companyId == x.CompanyId && sourceId == x.SourceId, Expression);
    }

    public DateFilter(string[] companyIds)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = x => companyIds.Contains(x.CompanyId);
    }
    public DateFilter(string[] companyIds, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public DateFilter(string[] companyIds, int year, int month) : base(year, month)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public DateFilter(HttpRequestFilterType filterType, string[] companyIds, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public DateFilter(string[] companyIds, byte sourceId, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        SourceId = sourceId;
        Expression = Combine(x => companyIds.Contains(x.CompanyId) && sourceId == x.SourceId, Expression);
    }
    public DateFilter(string[] companyIds, byte sourceId, int year, int month) : base(year, month)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        SourceId = sourceId;
        Expression = Combine(x => companyIds.Contains(x.CompanyId) && sourceId == x.SourceId, Expression);
    }
    public DateFilter(HttpRequestFilterType filterType, string[] companyIds, byte sourceId, int year, int month, int day) : base(filterType, year, month, day)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        SourceId = sourceId;
        Expression = Combine(x => companyIds.Contains(x.CompanyId) && sourceId == x.SourceId, Expression);
    }
}