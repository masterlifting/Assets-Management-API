using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Shared.SqlAccess.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;

using static IM.Service.Shared.Enums;
using static IM.Service.Shared.Helpers.ServiceHelper.ExpressionHelper;

namespace IM.Service.Market.Domain.DataAccess.Filters;

public class QuarterFilter<T> : FilterByQuarter<T> where T : class, IDataIdentity, IQuarterIdentity
{
    private string[] CompanyIds { get; } = Array.Empty<string>();
    private string? CompanyId { get; }
    private byte? SourceId { get; }

    private QuarterFilter(CompareType compareType, int year) : base(compareType, year) { }
    private QuarterFilter(CompareType compareType, int year, int quarter) : base(compareType, year, quarter) { }

    private QuarterFilter(CompareType compareType, string companyId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }
    private QuarterFilter(CompareType compareType, string companyId, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }

    private QuarterFilter(CompareType compareType, byte sourceId, int year) : base(compareType, year)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }
    private QuarterFilter(CompareType compareType, byte sourceId, int year, int quarter) : base(compareType, year, quarter)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }

    private QuarterFilter(CompareType compareType, string companyId, byte sourceId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => CompanyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }
    private QuarterFilter(CompareType compareType, string companyId, byte sourceId, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => CompanyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }

    private QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    private QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    private QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }
    private QuarterFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year, int quarter) : base(compareType, year, quarter)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }


    public static QuarterFilter<T> GetFilter(CompareType compareType, string? companyId, int? sourceId, int year = 0, int quarter = 0)
    {
        QuarterFilter<T> filter;

        switch (companyId)
        {
            case null when sourceId is null:
                filter = quarter != 0
                    ? new(compareType, year, quarter)
                    : new(compareType, year);
                break;
            case null when true:
                filter = quarter != 0
                    ? new(compareType, (byte)sourceId, year, quarter)
                    : new(compareType, (byte)sourceId, year);
                break;
            default:
            {
                var companyIds = companyId.Split(',');

                filter = sourceId is null
                    ? companyIds.Length > 1
                        ? quarter != 0
                            ? new(compareType, companyIds, year, quarter)
                            : new(compareType, companyIds, year)
                        : quarter != 0
                            ? new(compareType, companyId, year, quarter)
                            : new(compareType, companyId, year)
                    : companyIds.Length > 1
                        ? quarter != 0
                            ? new(compareType, companyIds, (byte)sourceId, year, quarter)
                            : new(compareType, companyIds, (byte)sourceId, year)
                        : quarter != 0
                            ? new(compareType, companyId, (byte)sourceId, year, quarter)
                            : new(compareType, companyId, (byte)sourceId, year);
                break;
            }
        }

        return filter;
    }
}