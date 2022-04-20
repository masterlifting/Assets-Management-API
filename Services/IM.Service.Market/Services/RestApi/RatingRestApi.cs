using System.Linq.Expressions;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Domain.DataAccess;
using IM.Service.Market.Domain.DataAccess.Filters;
using IM.Service.Market.Domain.Entities;
using IM.Service.Market.Models.Api.Http;

using Microsoft.EntityFrameworkCore;

namespace IM.Service.Market.Services.RestApi;

public class RatingRestApi
{
    private readonly Repository<Rating> ratingRepo;
    private readonly Repository<Company> companyRepo;
    private const int resultRoundValue = 3;

    public RatingRestApi(Repository<Rating> ratingRepo, Repository<Company> companyRepo)
    {
        this.ratingRepo = ratingRepo;
        this.companyRepo = companyRepo;
    }

    public async Task<ResponseModel<RatingGetDto>> GetAsync(int place)
    {
        var data = await ratingRepo
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

        return data is null
            ? new() { Errors = new[] { "rating not found" } }
            : new()
            {
                Data = new()
                {
                    Place = place,
                    Company = data.CompanyName,
                    Result = data.Result.HasValue ? decimal.Round(data.Result.Value, resultRoundValue) : null,
                    ResultPrice = data.ResultPrice.HasValue ? decimal.Round(data.ResultPrice.Value, resultRoundValue) : null,
                    ResultReport = data.ResultReport.HasValue ? decimal.Round(data.ResultReport.Value, resultRoundValue) : null,
                    ResultCoefficient = data.ResultCoefficient.HasValue ? decimal.Round(data.ResultCoefficient.Value, resultRoundValue) : null,
                    ResultDividend = data.ResultDividend.HasValue ? decimal.Round(data.ResultDividend.Value, resultRoundValue) : null,
                    UpdateTime = new DateTime(data.Date.Year, data.Date.Month, data.Date.Day, data.Time.Hour, data.Time.Minute, data.Time.Second)
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
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedRatingIds
            .Select((x, i) => new { Place = i + 1, CompanyId = x })
            .ToDictionary(x => x.CompanyId, y => y.Place);

        var result = await ratingRepo.FindAsync(x => x.CompanyId == company.Id);

        return result is null
            ? new() { Errors = new[] { "rating not found" } }
            : new()
            {
                Data = new()
                {
                    Company = company.Name,
                    Place = places[result.CompanyId],
                    Result = result.Result.HasValue ? decimal.Round(result.Result.Value, resultRoundValue) : null,
                    ResultPrice = result.ResultPrice.HasValue ? decimal.Round(result.ResultPrice.Value, resultRoundValue) : null,
                    ResultReport = result.ResultReport.HasValue ? decimal.Round(result.ResultReport.Value, resultRoundValue) : null,
                    ResultCoefficient = result.ResultCoefficient.HasValue ? decimal.Round(result.ResultCoefficient.Value, resultRoundValue) : null,
                    ResultDividend = result.ResultDividend.HasValue ? decimal.Round(result.ResultDividend.Value, resultRoundValue) : null,
                    UpdateTime = new DateTime(result.Date.Year, result.Date.Month, result.Date.Day, result.Time.Hour, result.Time.Minute, result.Time.Second)
                }
            };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetAsync(HttpPagination pagination, string entity)
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
        return company is null ? $"'{companyId}' not found" : await RecalculateBaseAsync(new[] { company.Id });
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
    public async Task<string> RecalculateAsync(CompanyDateFilter<Rating> filter)
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

        return /*result.error ??*/ $"Recalculate rating for '{string.Join(";", companyIds)}' at {date.Value: yyyy-MM-dd} is running.";
    }
}