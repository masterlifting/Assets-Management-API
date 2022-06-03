using IM.Service.Shared.Models.Entity.Interfaces;
using IM.Service.Shared.RepositoryService.Filters;
using IM.Service.Market.Domain.Entities.Interfaces;

using static IM.Service.Shared.Enums;
using static IM.Service.Shared.Helpers.ServiceHelper.ExpressionHelper;

namespace IM.Service.Market.Domain.DataAccess.Filters;

public class DateFilter<T> : FilterByDate<T> where T : class, IDateIdentity, IDataIdentity
{
    private string[] CompanyIds { get; } = Array.Empty<string>();
    private string? CompanyId { get; }
    private byte? SourceId { get; }

    private DateFilter(CompareType compareType, int year) : base(compareType, year) { }
    private DateFilter(CompareType compareType, int year, int month) : base(compareType, year, month) { }
    private DateFilter(CompareType compareType, int year, int month, int day) : base(compareType, year, month, day) { }

    private DateFilter(CompareType compareType, string companyId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }
    private DateFilter(CompareType compareType, string companyId, int year, int month) : base(compareType, year, month)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }
    private DateFilter(CompareType compareType, string companyId, int year, int month, int day) : base(compareType, year, month, day)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        Expression = Combine(x => CompanyId == x.CompanyId, Expression);
    }

    private DateFilter(CompareType compareType, byte sourceId, int year) : base(compareType, year)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }
    private DateFilter(CompareType compareType, byte sourceId, int year, int month) : base(compareType, year, month)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }
    private DateFilter(CompareType compareType, byte sourceId, int year, int month, int day) : base(compareType, year, month, day)
    {
        SourceId = sourceId;
        Expression = Combine(x => SourceId == x.SourceId, Expression);
    }

    private DateFilter(CompareType compareType, string companyId, byte sourceId, int year) : base(compareType, year)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => CompanyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }
    private DateFilter(CompareType compareType, string companyId, byte sourceId, int year, int month) : base(compareType, year, month)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => CompanyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }
    private DateFilter(CompareType compareType, string companyId, byte sourceId, int year, int month, int day) : base(compareType, year, month, day)
    {
        CompanyId = companyId.Trim().ToUpperInvariant();
        SourceId = sourceId;
        Expression = Combine(x => companyId == x.CompanyId && SourceId == x.SourceId, Expression);
    }

    private DateFilter(CompareType compareType, IEnumerable<string> companyIds, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    private DateFilter(CompareType compareType, IEnumerable<string> companyIds, int year, int month) : base(compareType, year, month)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    private DateFilter(CompareType compareType, IEnumerable<string> companyIds, int year, int month, int day) : base(compareType, year, month, day)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId), Expression);
    }
    private DateFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year) : base(compareType, year)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }
    private DateFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year, int month) : base(compareType, year, month)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }
    private DateFilter(CompareType compareType, IEnumerable<string> companyIds, byte sourceId, int year, int month, int day) : base(compareType, year, month, day)
    {
        CompanyIds = companyIds.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).ToArray();
        SourceId = sourceId;
        Expression = Combine(x => CompanyIds.Contains(x.CompanyId) && SourceId == x.SourceId, Expression);
    }


    public static DateFilter<T> GetFilter(CompareType compareType, string? companyId, int? sourceId, int year = 0, int month = 0, int day = 0)
    {
        DateFilter<T> filter;

        switch (companyId)
        {
            case null when sourceId is null:
                filter = day != 0
                    ? new(compareType, year, month, day)
                    : month != 0
                        ? new(compareType, year, month)
                        : new(compareType, year);
                break;
            case null when true:
                filter = day != 0
                    ? new(compareType, (byte)sourceId, year, month, day)
                    : month != 0
                        ? new(compareType, (byte)sourceId, year, month)
                        : new(compareType, (byte)sourceId, year);
                break;
            default:
                {
                    var companyIds = companyId.Split(',');

                    filter = sourceId is null
                        ? companyIds.Length > 1
                            ? day != 0
                                ? new(compareType, companyIds, year, month, day)
                                : month != 0
                                    ? new(compareType, companyIds, year, month)
                                    : new(compareType, companyIds, year)
                            : day != 0
                                ? new(compareType, companyId, year, month, day)
                                : month != 0
                                    ? new(compareType, companyId, year, month)
                                    : new(compareType, companyId, year)
                        : companyIds.Length > 1
                            ? day != 0
                                ? new(compareType, companyIds, (byte)sourceId, year, month, day)
                                : month != 0
                                    ? new(compareType, companyIds, (byte)sourceId, year, month)
                                    : new(compareType, companyIds, (byte)sourceId, year)
                            : day != 0
                                ? new(compareType, companyId, (byte)sourceId, year, month, day)
                                : month != 0
                                    ? new(compareType, companyId, (byte)sourceId, year, month)
                                    : new(compareType, companyId, (byte)sourceId, year);

                    break;
                }
        }

        return filter;
    }
}