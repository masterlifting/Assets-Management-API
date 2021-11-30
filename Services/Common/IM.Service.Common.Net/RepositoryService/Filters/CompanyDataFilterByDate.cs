using System;
using System.Linq.Expressions;
using IM.Service.Common.Net.Models.Entity.CompanyServices.Interfaces;
using static IM.Service.Common.Net.CommonEnums;


namespace IM.Service.Common.Net.RepositoryService.Filters;

public class CompanyDataFilterByDate<T> : DataFilterByDate<T> where T : class, ICompanyDateIdentity
{
    private readonly string? companyId;

    public CompanyDataFilterByDate(int year) : base(year) { }
    public CompanyDataFilterByDate(int year, int month) : base(year, month) { }
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, int year, int month, int day) : base(filterType, year, month, day) { }
    public CompanyDataFilterByDate(string companyId, int year) : base(year) =>
        this.companyId = companyId.ToUpperInvariant().Trim();
    public CompanyDataFilterByDate(string companyId, int year, int month) : base(year, month) =>
        this.companyId = companyId.ToUpperInvariant().Trim();
    public CompanyDataFilterByDate(HttpRequestFilterType filterType, string companyId, int year, int month, int day) : base(filterType, year, month, day) =>
        this.companyId = companyId.ToUpperInvariant().Trim();
    public Expression<Func<T, bool>> FilterExpression => Combine(CompanyExpression, DateExpression);

    private Expression<Func<T, bool>> CompanyExpression => x => companyId == null | companyId == x.CompanyId;
}