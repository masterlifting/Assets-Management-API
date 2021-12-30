﻿using System;
using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Analyzer.DataAccess.Comparators;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.DtoServices;

public class RatingDtoManager
{
    private readonly Repository<Rating> ratingRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<AnalyzedEntity> analyzedEntityRepository;

    public RatingDtoManager(
        Repository<AnalyzedEntity> analyzedEntityRepository,
        Repository<Rating> ratingRepository,
        Repository<DataAccess.Entities.Company> companyRepository)
    {
        this.analyzedEntityRepository = analyzedEntityRepository;
        this.ratingRepository = ratingRepository;
        this.companyRepository = companyRepository;
    }

    public async Task<ResponseModel<RatingGetDto>> GetAsync(int place)
    {
        var rating = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Skip(place - 1)
            .Take(1)
            .FirstAsync();

        var company = await companyRepository.FindAsync(rating.CompanyId);

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
                Result = decimal.Round(rating.Result, 1),
                ResultPrice = decimal.Round(rating.ResultPrice, 1),
                ResultReport = decimal.Round(rating.ResultReport, 1),
                ResultCoefficient = decimal.Round(rating.ResultCoefficient, 1),
                UpdateTime = rating.Date
            }
        };
    }
    public async Task<ResponseModel<RatingGetDto>> GetAsync(string companyId)
    {
        var company = await companyRepository.FindAsync(companyId.ToUpperInvariant());

        if (company is null)
            return new()
            {
                Errors = new[] { "company not found" }
            };

        var orderedRatingIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.Id)
            .ToArrayAsync();

        var places = orderedRatingIds
            .Select((x, i) => new { Place = i + 1, RatingId = x })
            .ToDictionary(x => x.RatingId, y => y.Place);

        var result = await ratingRepository.FindAsync(x => x.CompanyId == company.Id);

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
                Result = decimal.Round(result.Result, 1),
                ResultPrice = decimal.Round(result.ResultPrice, 1),
                ResultReport = decimal.Round(result.ResultReport, 1),
                ResultCoefficient = decimal.Round(result.ResultCoefficient, 1),
                UpdateTime = result.Date
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<RatingGetDto>>> GetAsync(HttpPagination pagination)
    {
        var count = await ratingRepository.GetCountAsync();
        var orderedCompanyIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.Result)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepository.GetPaginationQueryDesc(pagination, x => x.Result)
            .Join(companyRepository.GetDbSet(),
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
                    UpdateTime = x.Date
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = decimal.Round(x.Result, 1),
                    ResultPrice = decimal.Round(x.ResultPrice, 1),
                    ResultReport = decimal.Round(x.ResultReport, 1),
                    ResultCoefficient = decimal.Round(x.ResultCoefficient, 1),
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
        var count = await ratingRepository.GetCountAsync();
        var orderedCompanyIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.ResultPrice)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepository.GetPaginationQueryDesc(pagination, x => x.ResultPrice)
            .Join(companyRepository.GetDbSet(),
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
                    UpdateTime = x.Date
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = decimal.Round(x.Result, 1),
                    ResultPrice = decimal.Round(x.ResultPrice, 1),
                    ResultReport = decimal.Round(x.ResultReport, 1),
                    ResultCoefficient = decimal.Round(x.ResultCoefficient, 1),
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
        var count = await ratingRepository.GetCountAsync();
        var orderedCompanyIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.ResultReport)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepository.GetPaginationQueryDesc(pagination, x => x.ResultReport)
            .Join(companyRepository.GetDbSet(),
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
                    UpdateTime = x.Date
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = decimal.Round(x.Result, 1),
                    ResultPrice = decimal.Round(x.ResultPrice, 1),
                    ResultReport = decimal.Round(x.ResultReport, 1),
                    ResultCoefficient = decimal.Round(x.ResultCoefficient, 1),
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
        var count = await ratingRepository.GetCountAsync();
        var orderedCompanyIds = await ratingRepository
            .GetQuery()
            .OrderByDescending(x => x.ResultCoefficient)
            .Select(x => x.CompanyId)
            .ToArrayAsync();

        var places = orderedCompanyIds.Select((x, i) => new { Place = i + 1, CompanyId = x });

        var queryResult = await ratingRepository.GetPaginationQueryDesc(pagination, x => x.ResultCoefficient)
            .Join(companyRepository.GetDbSet(),
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
                    UpdateTime = x.Date
                })
            .ToArrayAsync();

        var result = queryResult.Join(places,
                x => x.CompanyId,
                y => y.CompanyId,
                (x, y) => new RatingGetDto
                {
                    Place = y.Place,
                    Company = x.Company,
                    Result = decimal.Round(x.Result, 1),
                    ResultPrice = decimal.Round(x.ResultPrice, 1),
                    ResultReport = decimal.Round(x.ResultReport, 1),
                    ResultCoefficient = decimal.Round(x.ResultCoefficient, 1),
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
        var company = await companyRepository.FindAsync(companyId.ToLowerInvariant());
        return company is null ? "company not found" : await RecalculateBaseAsync(new[] {company.Id});
    }
    public async Task<string> RecalculateAsync()
    {
        var ids = await companyRepository.GetSampleAsync(x => x.Id);
        return await RecalculateBaseAsync(ids);
    }
    public async Task<string> RecalculateAsync(string[] companyIds)
    {
        var ids = await companyRepository.GetSampleAsync(x => companyIds.Select(y => y.Trim().ToUpperInvariant()).Contains(x.Id), x => x.Id);
        return await RecalculateBaseAsync(ids);
    }
    public async Task<string> RecalculateAsync(CompanyDataFilterByDate<Rating> filter)
    {
        var ids = filter.CompanyId is null && !filter.CompanyIds.Any()
            ? await companyRepository.GetSampleAsync(x => x.Id)
            : await companyRepository.GetSampleAsync(x => filter.CompanyId != null ? filter.CompanyId == x.Id : filter.CompanyIds.Contains(x.Id), x => x.Id);
        
        return await RecalculateBaseAsync(ids, new DateTime(filter.Year, filter.Month, filter.Day));
    }
    private async Task<string> RecalculateBaseAsync(string[] companyIds, DateTime? date = null)
    {
        var types = Enum.GetValues<EntityTypes>();
        date ??= new DateTime(2016, 1, 1);

        var data = companyIds.SelectMany(x => types.Select(y => new AnalyzedEntity
        {
            CompanyId = x,
            AnalyzedEntityTypeId = (byte)y,
            StatusId = (byte)Statuses.Ready,
            Date = date.Value
        }));

        var result = await analyzedEntityRepository.CreateUpdateAsync(data, new AnalyzedEntityComparer(), nameof(RecalculateAsync));

        return result.error ?? $"task to recalculate rating for '{string.Join(";",companyIds)}' at {date.Value : yyyy-MM-dd} is running.";
    }
}