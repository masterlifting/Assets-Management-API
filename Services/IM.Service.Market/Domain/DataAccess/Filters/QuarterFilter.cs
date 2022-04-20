using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helper.ExpressionHelper;

namespace IM.Service.Market.Domain.DataAccess.Filters;

public class QuarterFilter<T> : FilterByQuarter<T> where T : class, IDataIdentity, IQuarterIdentity
{
    private string[] CompanyIds { get; } = Array.Empty<string>();
    private string? CompanyId { get; }
    private byte? SourceId { get; }

    public QuarterFilter(CompareType compareType, int year) : base(compareType, year) { }
    public QuarterFilter(CompareType compareType, int year, int quarter) : base(compareType, year, quarter) { }

    public QuarterFilter(CompareType compareType, string companyId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }
    public QuarterFilter(CompareType compareType, string companyId, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }

    public QuarterFilter(CompareType compareType, byte sourceId, int year) : base(compareType, year)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }
    public QuarterFilter(CompareType compareType, byte sourceId, int year, int quarter) : base(compareType, year, quarter)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }

    public QuarterFilter(CompareType compareType, string companyId, byte sourceId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => CompanyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }
    public QuarterFilter(CompareType compareType, string companyId, byte sourceId, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => CompanyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }

    public QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    public QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    public QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }
    public QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }
}