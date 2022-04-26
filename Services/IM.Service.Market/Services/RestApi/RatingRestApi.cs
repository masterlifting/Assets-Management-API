using IM.Service.Common.Net.Models.Entity.Interfaces;
using IM.Service.Common.Net.Models.Services;
using IM.Service.Common.Net.RepositoryService;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Domain.Entities.Interfaces;
using IM.Service.Market.Models.Api.Http;

using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
using System.Text;

using static IM.Service.Common.Net.Enums;
using static IM.Service.Common.Net.Helpers.LogicHelper;
using static IM.Service.Common.Net.Helpers.ServiceHelper;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Services.RestApi;

public class RatingRestApi
{
    private readonly Repository<Rating> ratingRepo;
    private readonly Repository<Company> companyRepo;
    private readonly Repository<Price> priceRepo;
    private readonly Repository<Report> reportRepo;
    private readonly Repository<Coefficient> coefficientRepo;
    private readonly Repository<Dividend> dividendRepo;
    private const int resultRoundValue = 3;

    public RatingRestApi(
        Repository<Rating> ratingRepo,
        Repository<Company> companyRepo,
        Repository<Price> priceRepo,
        Repository<Report> reportRepo,
        Repository<Coefficient> coefficientRepo,
        Repository<Dividend> dividendRepo)
    {
        this.ratingRepo = ratingRepo;
        this.companyRepo = companyRepo;
        this.priceRepo = priceRepo;
        this.reportRepo = reportRepo;
        this.coefficientRepo = coefficientRepo;
        this.dividendRepo = dividendRepo;
    }

    public async Task<RatingGetDto> GetAsync(int place)
    {
        var rating = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Skip(place - 1)
            .Take(1)
            .Select(x => new
            {
                CompanyName = x.Company.Name,
                x.Result,
                x.ResultPrice,
                x.ResultReport,
                x.ResultCoefficient,
                x.ResultDividend,
                x.Date,
                x.Time
            })
            .FirstOrDefaultAsync();

        return rating is null
            ? throw new NullReferenceException(nameof(rating))
            : new RatingGetDto
            {
                Place = place,
                Company = rating.CompanyName,
                Result = rating.Result.HasValue ? decimal.Round(rating.Result.Value, resultRoundValue) : null,
                ResultPrice = rating.ResultPrice.HasValue ? decimal.Round(rating.ResultPrice.Value, resultRoundValue) : null,
                ResultReport = rating.ResultReport.HasValue ? decimal.Round(rating.ResultReport.Value, resultRoundValue) : null,
                ResultCoefficient = rating.ResultCoefficient.HasValue ? decimal.Round(rating.ResultCoefficient.Value, resultRoundValue) : null,
                ResultDividend = rating.ResultDividend.HasValue ? decimal.Round(rating.ResultDividend.Value, resultRoundValue) : null,
                UpdateTime = new DateTime(rating.Date.Year, rating.Date.Month, rating.Date.Day, rating.Time.Hour, rating.Time.Minute, rating.Time.Second)
            };
    }
    public async Task<RatingGetDto> GetAsync(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();

        var company = await companyRepo.FindAsync(companyId);

        if (company is null)
            throw new NullReferenceException(nameof(company));

        var orderedRatingIds = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedRatingIds
            .Select((x, i) => new { Place = i + 1, CompanyId = x })
            .ToDictionary(x => x.CompanyId, y => y.Place);

        var rating = await ratingRepo.FindAsync(x => x.CompanyId == company.Id);

        return rating is null
            ? throw new NullReferenceException(nameof(rating))
            : new RatingGetDto
            {
                Company = company.Name,
                Place = places[rating.CompanyId],
                Result = rating.Result.HasValue ? decimal.Round(rating.Result.Value, resultRoundValue) : null,
                ResultPrice = rating.ResultPrice.HasValue ? decimal.Round(rating.ResultPrice.Value, resultRoundValue) : null,
                ResultReport = rating.ResultReport.HasValue ? decimal.Round(rating.ResultReport.Value, resultRoundValue) : null,
                ResultCoefficient = rating.ResultCoefficient.HasValue ? decimal.Round(rating.ResultCoefficient.Value, resultRoundValue) : null,
                ResultDividend = rating.ResultDividend.HasValue ? decimal.Round(rating.ResultDividend.Value, resultRoundValue) : null,
                UpdateTime = new DateTime(rating.Date.Year, rating.Date.Month, rating.Date.Day, rating.Time.Hour, rating.Time.Minute, rating.Time.Second)
            };
    }
    public async Task<PaginationModel<RatingGetDto>> GetAsync(Paginatior pagination, string entity)
    {
        var placeStart = (pagination.Page - 1) * pagination.Limit + 1;

        Expression<Func<Rating, decimal?>> orderSelector = entity switch
        {
            nameof(Rating) => x => x.Result,
            nameof(Price) => x => x.ResultPrice,
            nameof(Report) => x => x.ResultReport,
            nameof(Coefficient) => x => x.ResultCoefficient,
            nameof(Dividend) => x => x.ResultDividend,
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, null)
        };

        var count = await ratingRepo.GetCountAsync();
        var data = await ratingRepo
            .GetPaginationQueryDesc(pagination, orderSelector)
            .Select(x => new
            {
                CompanyName = x.Company.Name,
                x.Result,
                x.ResultPrice,
                x.ResultReport,
                x.ResultCoefficient,
                x.ResultDividend,
                x.Date,
                x.Time
            })
            .ToArrayAsync();

        var result = data
            .Select((x, i) => new RatingGetDto
            {
                Place = placeStart + i,
                Company = x.CompanyName,
                Result = x.Result.HasValue ? decimal.Round(x.Result.Value, resultRoundValue) : null,
                ResultPrice = x.ResultPrice.HasValue ? decimal.Round(x.ResultPrice.Value, resultRoundValue) : null,
                ResultReport = x.ResultReport.HasValue ? decimal.Round(x.ResultReport.Value, resultRoundValue) : null,
                ResultCoefficient = x.ResultCoefficient.HasValue ? decimal.Round(x.ResultCoefficient.Value, resultRoundValue) : null,
                ResultDividend = x.ResultDividend.HasValue ? decimal.Round(x.ResultDividend.Value, resultRoundValue) : null,
                UpdateTime = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Time.Hour, x.Time.Minute, x.Time.Second)
            })
            .ToArray();

        return new PaginationModel<RatingGetDto> { Items = result, Count = count };
    }

    public async Task<string> RecalculateAsync(CompareType compareType, string? companyId, int year = 0, int month = 0, int day = 0)
    {
        var result = new StringBuilder(500);

        result.Append("Recalculating task start");

        var priceFilter = DateFilter<Price>.GetFilter(compareType, companyId, null, year, month, day);
        var dividendFilter = DateFilter<Dividend>.GetFilter(compareType, companyId, null, year, month, day);
        var reportFilter = QuarterFilter<Report>.GetFilter(compareType, companyId, null, year, QuarterHelper.GetQuarter(month));
        var coefficientFilter = QuarterFilter<Coefficient>.GetFilter(compareType, companyId, null, year, QuarterHelper.GetQuarter(month));

        var prices = await priceRepo.GetSampleAsync(priceFilter.Expression);
        var dividends = await dividendRepo.GetSampleOrderedAsync(dividendFilter.Expression, x => x.Date);
        var reports = await reportRepo.GetSampleOrderedAsync(reportFilter.Expression, x => x.Year, x => x.Quarter);
        var coefficients = await coefficientRepo.GetSampleOrderedAsync(coefficientFilter.Expression, x => x.Year, x => x.Quarter);

        await SetCalculatingTask(priceRepo, prices, result, x => x.Date);
        await SetCalculatingTask(reportRepo, reports, result, x => x.Year, x => x.Quarter);
        await SetCalculatingTask(dividendRepo, dividends, result, x => x.Date);
        await SetCalculatingTask(coefficientRepo, coefficients, result, x => x.Year, x => x.Quarter);

        return result.ToString();
    }
    private static async Task SetCalculatingTask<T, TSelector>(Repository<T, DatabaseContext> repository, T[] data, StringBuilder result, Func<T, TSelector> orderSelector, Func<T, TSelector>? orderSelector2 = null) where T : class, IRating, IPeriod
    {
        if (!data.Any())
            return;

        var groupedData = data.GroupBy(x => x.CompanyId).ToArray();
        var dataResult = new List<T>(groupedData.Length);

        foreach (var group in groupedData)
        {
            var firstData = orderSelector2 is null
                ? group.OrderBy(orderSelector.Invoke).First()
                : group.OrderBy(orderSelector.Invoke).ThenBy(orderSelector2.Invoke).First();
            firstData.StatusId = (byte)Statuses.Ready;
            dataResult.Add(firstData);
        }

        await repository.UpdateAsync(dataResult, "Recalculating rating");

        result.Append($"Recalculating rating task for {typeof(T).Name}: {string.Join("; ", dataResult.Select(x => x.CompanyId))} is running");
    }
}