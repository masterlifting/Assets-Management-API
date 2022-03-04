using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.DataAccess.Filters;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Models.Api.Http;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Data.Services.RestApi;

public class RatingApi
{
    private readonly Repository<Rating> ratingRepo;
    private readonly Repository<Company> companyRepo;
    private const int resultRoundValue = 3;

    public RatingApi(
        Repository<Rating> ratingRepo,
        Repository<Company> companyRepo)
    {
        this.ratingRepo = ratingRepo;
        this.companyRepo = companyRepo;
    }

    public async Task<ResponseModel<RatingGetDto>> GetAsync(int place)
    {
        var rating = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Skip(place - 1)
            .Take(1)
            .FirstAsync();

        var company = await companyRepo.FindAsync(rating.CompanyId);

        if (company is null)
            return new()
            {
                Errors = new[] { "company not found" }
            };

        return new()
        {
            Data = new()
            {
                Company = company.Name,
                Place = place,
                Result =rating.Result.HasValue ? decimal.Round(rating.Result.Value, resultRoundValue) : null,
                ResultPrice = rating.ResultPrice.HasValue ? decimal.Round(rating.ResultPrice.Value, resultRoundValue) : null,
                ResultReport = rating.ResultReport.HasValue ? decimal.Round(rating.ResultReport.Value, resultRoundValue) : null,
                ResultCoefficient = rating.ResultCoefficient.HasValue ? decimal.Round(rating.ResultCoefficient.Value, resultRoundValue) : null,
                UpdateTime = new DateTime(rating.Date.Year,rating.Date.Month,rating.Date.Day,rating.Time.Hour,rating.Time.Minute,rating.Time.Second)

            }
        };
    }
    public async Task<ResponseModel<RatingGetDto>> GetAsync(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var company = await companyRepo.FindAsync(companyId);

        if (company is null)
            return new()
            {
                Errors = new[] { "company not found" }
            };

        var orderedRatingIds = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.Id)
            .ToArrayAsync();

        var places = orderedRatingIds
            .Select((x, i) => new { Place = i + 1, RatingId = x })
            .ToDictionary(x => x.RatingId, y => y.Place);

        var result = await ratingRepo.FindAsync(x => x.CompanyId == company.Id);

        if (result is null)
            return new()
            {
                Errors = new[] { "rating not found" }
            };

        return new()
        {
            Data = new()
            {
                Company = company.Name,
                Place = places[result.Id],
                Result = result.Result.HasValue ? decimal.Round(result.Result.Value, resultRoundValue) : null,
                ResultPrice = result.ResultPrice.HasValue ? decimal.Round(result.ResultPrice.Value, resultRoundValue) : null,
                ResultReport = result.ResultReport.HasValue ? decimal.Round(result.ResultReport.Value, resultRoundValue) : null,
                ResultCoefficient = result.ResultCoefficient.HasValue ? decimal.Round(result.ResultCoefficient.Value, resultRoundValue) : null,
                UpdateTime = new DateTime(result.Date.Year, result.Date.Month, result.Date.Day, result.Time.Hour, result.Time.Minute, result.Time.Second)
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await ratingRepo.GetCountAsync();
        var orderedCompanyIds = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepo.GetPaginationQueryDesc(pagination, x => x.Result)
            .Join(companyRepo.GetDbSet(),
                x => x.CompanyId,
                y => y.Id,
                (x, y) => new
                {
                    Company = y.Name,
                    x.CompanyId,
                    x.Result,
                    x.ResultPrice,
                    x.ResultReport,
                    x.ResultCoefficient,
                    UpdateTime = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Time.Hour, x.Time.Minute, x.Time.Second)
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = x.Result.HasValue ? decimal.Round(x.Result.Value, resultRoundValue) : null,
                    ResultPrice = x.ResultPrice.HasValue ? decimal.Round(x.ResultPrice.Value, resultRoundValue) : null,
                    ResultReport = x.ResultReport.HasValue ? decimal.Round(x.ResultReport.Value, resultRoundValue) : null,
                    ResultCoefficient = x.ResultCoefficient.HasValue ? decimal.Round(x.ResultCoefficient.Value, resultRoundValue) : null,
                    UpdateTime = x.UpdateTime
                })
            .ToArray();


        return new()
        {
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetPriceResultOrderedAsync(HttpPagination pagination)
    {
        var count = await ratingRepo.GetCountAsync();
        var orderedCompanyIds = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.ResultPrice)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepo.GetPaginationQueryDesc(pagination, x => x.ResultPrice)
            .Join(companyRepo.GetDbSet(),
                x => x.CompanyId,
                y => y.Id,
                (x, y) => new
                {
                    Company = y.Name,
                    x.CompanyId,
                    x.Result,
                    x.ResultPrice,
                    x.ResultReport,
                    x.ResultCoefficient,
                    UpdateTime = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Time.Hour, x.Time.Minute, x.Time.Second)
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = x.Result.HasValue ? decimal.Round(x.Result.Value, resultRoundValue) : null,
                    ResultPrice = x.ResultPrice.HasValue ? decimal.Round(x.ResultPrice.Value, resultRoundValue) : null,
                    ResultReport = x.ResultReport.HasValue ? decimal.Round(x.ResultReport.Value, resultRoundValue) : null,
                    ResultCoefficient = x.ResultCoefficient.HasValue ? decimal.Round(x.ResultCoefficient.Value, resultRoundValue) : null,
                    UpdateTime = x.UpdateTime
                })
            .ToArray();


        return new()
        {
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetReportResultOrderedAsync(HttpPagination pagination)
    {
        var count = await ratingRepo.GetCountAsync();
        var orderedCompanyIds = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.ResultReport)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepo.GetPaginationQueryDesc(pagination, x => x.ResultReport)
            .Join(companyRepo.GetDbSet(),
                x => x.CompanyId,
                y => y.Id,
                (x, y) => new
                {
                    Company = y.Name,
                    x.CompanyId,
                    x.Result,
                    x.ResultPrice,
                    x.ResultReport,
                    x.ResultCoefficient,
                    UpdateTime = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Time.Hour, x.Time.Minute, x.Time.Second)
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = x.Result.HasValue ? decimal.Round(x.Result.Value, resultRoundValue) : null,
                    ResultPrice = x.ResultPrice.HasValue ? decimal.Round(x.ResultPrice.Value, resultRoundValue) : null,
                    ResultReport = x.ResultReport.HasValue ? decimal.Round(x.ResultReport.Value, resultRoundValue) : null,
                    ResultCoefficient = x.ResultCoefficient.HasValue ? decimal.Round(x.ResultCoefficient.Value, resultRoundValue) : null,
                    UpdateTime = x.UpdateTime
                })
            .ToArray();


        return new()
        {
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetCoefficientResultOrderedAsync(HttpPagination pagination)
    {
        var count = await ratingRepo.GetCountAsync();
        var orderedCompanyIds = await ratingRepo
            .GetQuery()
            .OrderByDescending(x => x.ResultCoefficient)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepo.GetPaginationQueryDesc(pagination, x => x.ResultCoefficient)
            .Join(companyRepo.GetDbSet(),
                x => x.CompanyId,
                y => y.Id,
                (x, y) => new
                {
                    Company = y.Name,
                    x.CompanyId,
                    x.Result,
                    x.ResultPrice,
                    x.ResultReport,
                    x.ResultCoefficient,
                    UpdateTime = new DateTime(x.Date.Year, x.Date.Month, x.Date.Day, x.Time.Hour, x.Time.Minute, x.Time.Second)
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = x.Result.HasValue ? decimal.Round(x.Result.Value, resultRoundValue) : null,
                    ResultPrice = x.ResultPrice.HasValue ? decimal.Round(x.ResultPrice.Value, resultRoundValue) : null,
                    ResultReport = x.ResultReport.HasValue ? decimal.Round(x.ResultReport.Value, resultRoundValue) : null,
                    ResultCoefficient = x.ResultCoefficient.HasValue ? decimal.Round(x.ResultCoefficient.Value, resultRoundValue) : null,
                    UpdateTime = x.UpdateTime
                })
            .ToArray();
        
        return new()
        {
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }

    public async Task<string> RecalculateAsync(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var company = await companyRepo.FindAsync(companyId);
        return company is null ? $"'{companyId}' not found" : await RecalculateBaseAsync(new[] {company.Id});
    }
    public async Task<string> RecalculateAsync()
    {
        var ids = await companyRepo.GetSampleAsync(x => x.Id);
        return await RecalculateBaseAsync(ids);
    }
    public async Task<string> RecalculateAsync(string[] companyIds)
    {
        var ids = await companyRepo.GetSampleAsync(x => companyIds.Select(y => y.Trim().ToUpperInvariant()).Contains(x.Id), x => x.Id);
        return await RecalculateBaseAsync(ids);
    }
    public async Task<string> RecalculateAsync(CompanySourceDateFilter<Rating> filter)
    {
        var ids = filter.CompanyId is null && !filter.CompanyIds.Any()
            ? await companyRepo.GetSampleAsync(x => x.Id)
            : await companyRepo.GetSampleAsync(x => filter.CompanyId != null ? filter.CompanyId == x.Id : filter.CompanyIds.Contains(x.Id), x => x.Id);
        
        return await RecalculateBaseAsync(ids, new DateOnly(filter.Year, filter.Month, filter.Day));
    }
    private async Task<string> RecalculateBaseAsync(string[] companyIds, DateOnly? date = null)
    {
        //var types = Enum.GetValues<Enums.EntityTypes>();
        //date ??= new DateOnly(2016, 1, resultRoundValue);

        //var data = companyIds.SelectMany(x => types.Select(y => new AnalyzedEntity
        //{
        //    CompanyId = x,
        //    AnalyzedEntityTypeId = (byte)y,
        //    StatusId = (byte)Enums.Statuses.Ready,
        //    Date = date.Value
        //}));

        //var result = await analyzedEntityRepository.CreateUpdateAsync(data, new AnalyzedEntityComparer(), nameof(RecalculateAsync));

        return /*result.error ??*/ $"Recalculate rating for '{string.Join(";",companyIds)}' at {date.Value : yyyy-MM-dd} is running.";
    }
}