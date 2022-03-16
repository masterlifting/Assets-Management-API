using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.MarketData.Domain.Entities.Interfaces;
using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helper.ExpressionHelper;

namespace IM.Service.MarketData.Domain.DataAccess.Filters;

public class QuarterFilter<T> : FilterByQuarter<T> where T : class, IDataIdentity, IQuarterIdentity
{
    public string[] CompanyIds { get; } = Array.Empty<string>();
    public string? CompanyId { get; }
    public byte? SourceId { get; }

    public QuarterFilter(int year) : base(year) { }
    public QuarterFilter(HttpRequestFilterType filterType, int year, int quarter) : base(filterType, year, quarter) { }

    public QuarterFilter(string companyId)
    {
        CompanyId = companyId;
        Expression = x => companyId == x.CompanyId;
    }
    public QuarterFilter(string companyId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }
    public QuarterFilter(HttpRequestFilterType filterType, string companyId, int year, int quarter) : base(filterType, year, quarter)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        Expression = Combine(x => companyId == x.CompanyId, Expression);
    }

    public QuarterFilter(byte sourceId)
    {
        SourceId = sourceId;
        Expression = x => sourceId == x.SourceId;
    }
    public QuarterFilter(byte sourceId, int year) : base(year)
    {
        SourceId = sourceId;
        Expression = Combine(x => sourceId == x.SourceId, Expression);
    }
    public QuarterFilter(HttpRequestFilterType filterType, byte sourceId, int year, int quarter) : base(filterType, year, quarter)
    {
        SourceId = sourceId;
        Expression = Combine(x => sourceId == x.SourceId, Expression);
    }

    public QuarterFilter(string companyId, byte sourceId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = x => companyId == x.CompanyId && sourceId == x.SourceId;
    }
    public QuarterFilter(string companyId, byte sourceId, int year) : base(year)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        SourceId = sourceId;
        Expression = Combine(x => companyId == x.CompanyId && sourceId == x.SourceId, Expression);
    }
    public QuarterFilter(HttpRequestFilterType filterType, string companyId, byte sourceId, int year, int quarter) : base(filterType, year, quarter)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        CompanyId = companyId;
        SourceId = sourceId;
        Expression = Combine(x => companyId == x.CompanyId && sourceId == x.SourceId, Expression);
    }

    public QuarterFilter(string[] companyIds)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = x => companyIds.Contains(x.CompanyId);
    }
    public QuarterFilter(string[] companyIds, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public QuarterFilter(HttpRequestFilterType filterType, string[] companyIds, int year, int quarter) : base(filterType, year, quarter)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        Expression = Combine(x => companyIds.Contains(x.CompanyId), Expression);
    }
    public QuarterFilter(string[] companyIds, byte sourceId, int year) : base(year)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        SourceId = sourceId;
        Expression = Combine(x => companyIds.Contains(x.CompanyId) && sourceId == x.SourceId, Expression);
    }
    public QuarterFilter(HttpRequestFilterType filterType, string[] companyIds, byte sourceId, int year, int quarter) : base(filterType, year, quarter)
    {
        companyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        CompanyIds = companyIds;
        SourceId = sourceId;
        Expression = Combine(x => companyIds.Contains(x.CompanyId) && sourceId == x.SourceId, Expression);
    }
}