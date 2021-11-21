using System;
using System.Linq.Expressions;
using IM.Service.Common.Net.Models.Entity.Companies.Interfaces;
using static IM.Service.Common.Net.CommonEnums;


namespace IM.Service.Common.Net.RepositoryService.Filters;

public class CompanyDataFilterByQuarter<T> : DataFilterByQuarter<T> where T : class, ICompanyQuarterIdentity
{
    private readonly string? companyId;

    public CompanyDataFilterByQuarter(int year) : base(year) { }
    public CompanyDataFilterByQuarter(int year, int quarter) : base(year, quarter) { }
    public CompanyDataFilterByQuarter(HttpRequestFilterType filterType, int year, int quarter) : base(filterType, year, quarter) { }
    public CompanyDataFilterByQuarter(string companyId, int year) : base(year) =>
        this.companyId = companyId.ToUpperInvariant().Trim();
    public CompanyDataFilterByQuarter(string companyId, int year, int quarter) : base(year, quarter) =>
        this.companyId = companyId.ToUpperInvariant().Trim();
    public CompanyDataFilterByQuarter(HttpRequestFilterType filterType, string companyId, int year, int quarter) : base(filterType, year, quarter) =>
        this.companyId = companyId.ToUpperInvariant().Trim();

    public Expression<Func<T, bool>> FilterExpression => Combine(CompanyExpression, QuarterExpression);

    private Expression<Func<T, bool>> CompanyExpression => x => companyId == null | companyId == x.CompanyId;
}