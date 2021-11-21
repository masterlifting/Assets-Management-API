using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Data.Services.DtoServices;

public class PricesDtoManager
{
    private readonly RepositorySet<Price> priceRepository;
    private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
    private readonly RepositorySet<StockSplit> stockSplitRepository;
    private readonly RepositorySet<StockVolume> stockVolumeRepository;
    public PricesDtoManager(
        RepositorySet<Price> priceRepository,
        RepositorySet<DataAccess.Entities.Company> companyRepository,
        RepositorySet<StockSplit> stockSplitRepository,
        RepositorySet<StockVolume> stockVolumeRepository)
    {
        this.priceRepository = priceRepository;
        this.companyRepository = companyRepository;
        this.stockSplitRepository = stockSplitRepository;
        this.stockVolumeRepository = stockVolumeRepository;
    }

    public async Task<ResponseModel<PriceGetDto>> GetAsync(string companyId, DateTime date)
    {
        var company = await companyRepository.FindAsync(companyId.ToUpperInvariant().Trim());

        if (company is null)
            return new() { Errors = new[] { "company not found" } };

        var price = await priceRepository.FindAsync(company.Id, date);

        if (price is null)
            return new() { Errors = new[] { "price not found" } };

        var stockSplits = await stockSplitRepository.GetSampleAsync(x => x.CompanyId == company.Id && x.Date <= date);
        var stockVolumes = await stockVolumeRepository.GetSampleOrderedAsync(x => x.CompanyId == company.Id && x.Date <= date, y => y.Date);

        return new()
        {
            Data = new()
            {
                Ticker = company.Id,
                Company = company.Name,
                Date = price.Date,
                SourceType = price.SourceType,
                Value = price.Value,
                ValueTrue = !stockSplits.Any() ? price.Value : price.Value * stockSplits.Aggregate(1, (current, item) => current * item.Value),
                StockVolume = !stockVolumes.Any() ? default : stockVolumes[^1].Value
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetAsync(CompanyDataFilterByDate<Price> filter, HttpPagination pagination)
    {
        var filteredQuery = priceRepository.GetQuery(filter.FilterExpression);
        var count = await priceRepository.GetCountAsync(filteredQuery);
        var paginatedQuery = priceRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

        var queryResult = await paginatedQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new
            {
                Company = y.Name,
                x.CompanyId,
                x.Date,
                x.Value,
                x.SourceType
            })
            .ToArrayAsync();

        var result = new List<PriceGetDto>(queryResult.Length);

        if (result.Any())
        {
            var priceResult = queryResult
                .OrderBy(x => x.Date)
                .Select(x => new Price
                {
                    CompanyId = x.CompanyId,
                    Date = x.Date,
                    Value = x.Value,
                    SourceType = x.SourceType
                })
                .ToArray();

            var date = priceResult[^1].Date;

            var companyNamesDictionary = queryResult
                .GroupBy(x => x.CompanyId)
                .ToDictionary(x => x.Key, y => y.First().Company);

            var stockSplits = await stockSplitRepository.GetSampleAsync(x => companyNamesDictionary.Keys.Contains(x.CompanyId) && x.Date <= date);

            var stockVolumes = await stockVolumeRepository.GetSampleAsync(x => companyNamesDictionary.Keys.Contains(x.CompanyId) && x.Date <= date);
            var stockVolumesDictionary = stockVolumes
                .GroupBy(x => x.CompanyId)
                .ToDictionary(x => x.Key);

            foreach (var groupSplits in stockSplits.GroupBy(x => x.CompanyId))
            {
                var companyName = companyNamesDictionary[groupSplits.Key];
                var targetStockVolumes = stockVolumesDictionary[groupSplits.Key].OrderBy(x => x.Date);

                var groupedSplits = groupSplits.OrderByDescending(x => x.Date).ToArray();

                foreach (var split in groupedSplits)
                {
                    var priceSplitResult = priceResult.Where(x => x.CompanyId == groupSplits.Key && x.Date >= split.Date).ToArray();
                    var stockSplitValue = groupedSplits.Where(x => x.Date <= split.Date).Aggregate(1, (current, x) => current * x.Value);

                    result.AddRange(priceSplitResult.Select(x => new PriceGetDto
                    {
                        Ticker = groupSplits.Key,
                        Company = companyName,
                        StockVolume = targetStockVolumes.LastOrDefault(y => y.Date <= x.Date)?.Value,
                        ValueTrue = x.Value * stockSplitValue,
                        Value = x.Value,
                        Date = x.Date,
                        SourceType = x.SourceType
                    }));

                    priceResult = priceResult.Except(priceSplitResult, new CompanyDateComparer<Price>()).ToArray();
                }
            }

            foreach (var group in priceResult.GroupBy(x => x.CompanyId))
            {
                var companyName = companyNamesDictionary[group.Key];
                var targetStockVolumes = stockVolumesDictionary[group.Key].OrderBy(x => x.Date);

                result.AddRange(group.Select(x => new PriceGetDto
                {
                    Ticker = group.Key,
                    Company = companyName,
                    StockVolume = targetStockVolumes.LastOrDefault(y => y.Date <= x.Date)?.Value,
                    Date = x.Date,
                    SourceType = x.SourceType,
                    Value = x.Value,
                    ValueTrue = x.Value
                }));
            }
        }

        return new()
        {
            Data = new()
            {
                Items = result.ToArray(),
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLastAsync(CompanyDataFilterByDate<Price> filter, HttpPagination pagination)
    {
        var filteredQuery = priceRepository.GetQuery(filter.FilterExpression);

        var queryResult = await filteredQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new
            {
                Company = y.Name,
                x.CompanyId,
                x.Date,
                x.Value,
                x.SourceType
            })
            .ToArrayAsync();

        var groupedResult = queryResult
            .GroupBy(x => x.CompanyId)
            .Select(x => x
                .OrderBy(y => y.Date)
                .Last())
            .ToArray();

        var paginatedResult = pagination.GetPaginatedResult(groupedResult);

        var result = new List<PriceGetDto>(paginatedResult.Length);

        if (result.Any())
        {
            var priceResult = paginatedResult
                .OrderBy(x => x.Date)
                .Select(x => new Price
                {
                    CompanyId = x.CompanyId,
                    Date = x.Date,
                    Value = x.Value,
                    SourceType = x.SourceType
                })
                .ToArray();

            var date = priceResult[^1].Date;

            var companyNamesDictionary = paginatedResult.ToDictionary(x => x.CompanyId, y => y.Company);

            var stockSplits = await stockSplitRepository.GetSampleAsync(x => companyNamesDictionary.Keys.Contains(x.CompanyId) && x.Date <= date);

            var stockVolumes = await stockVolumeRepository.GetSampleAsync(x => companyNamesDictionary.Keys.Contains(x.CompanyId) && x.Date <= date);
            var stockVolumesDictionary = stockVolumes
                .GroupBy(x => x.CompanyId)
                .ToDictionary(x => x.Key);

            foreach (var groupSplits in stockSplits.GroupBy(x => x.CompanyId))
            {
                var companyName = companyNamesDictionary[groupSplits.Key];
                var targetStockVolumes = stockVolumesDictionary[groupSplits.Key].OrderBy(x => x.Date);
                var groupedSplits = groupSplits.OrderByDescending(x => x.Date).ToArray();

                foreach (var split in groupedSplits)
                {
                    var priceSplitResult = priceResult.Where(x => x.CompanyId == groupSplits.Key && x.Date >= split.Date).ToArray();
                    var stockSplitValue = groupedSplits.Aggregate(1, (current, x) => current * x.Value);

                    result.AddRange(priceSplitResult.Select(x => new PriceGetDto
                    {
                        Ticker = groupSplits.Key,
                        Company = companyName,
                        StockVolume = targetStockVolumes.LastOrDefault(y => y.Date <= x.Date)?.Value,
                        ValueTrue = x.Value * stockSplitValue,
                        Value = x.Value,
                        Date = x.Date,
                        SourceType = x.SourceType
                    }));

                    priceResult = priceResult.Except(priceSplitResult, new CompanyDateComparer<Price>()).ToArray();
                }
            }

            result.AddRange(priceResult.Select(x => new PriceGetDto
            {
                Ticker = x.CompanyId,
                Company = companyNamesDictionary[x.CompanyId],
                StockVolume = stockVolumesDictionary[x.CompanyId].OrderBy(y => y.Date).LastOrDefault(y => y.Date <= x.Date)?.Value,
                Date = x.Date,
                SourceType = x.SourceType,
                Value = x.Value,
                ValueTrue = x.Value
            }));
        }

        return new()
        {
            Data = new()
            {
                Items = result.ToArray(),
                Count = result.Count
            }
        };
    }

    public async Task<ResponseModel<string>> CreateAsync(PricePostDto model)
    {
        var ctxEntity = new Price
        {
            CompanyId = model.CompanyId,
            SourceType = model.SourceType,
            Date = model.Date.Date,
            Value = model.Value
        };
        var message = $"price of '{model.CompanyId}' create at {model.Date:yyyy MMMM dd}";
        var (error, _) = await priceRepository.CreateAsync(ctxEntity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<PricePostDto> models)
    {
        var prices = models.ToArray();

        if (!prices.Any())
            return new() { Errors = new[] { "price data for creating not found" } };

        var ctxEntities = prices.GroupBy(x => x.Date.Date).Select(x => new Price
        {
            CompanyId = x.Last().CompanyId,
            SourceType = x.Last().SourceType,
            Date = x.Last().Date.Date,
            Value = x.Last().Value
        });

        var (error, result) = await priceRepository.CreateAsync(ctxEntities, new CompanyDateComparer<Price>(), "Prices");

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = $"Prices count: {result!.Length} was successed" };
    }
    public async Task<ResponseModel<string>> UpdateAsync(PricePostDto model)
    {
        var ctxEntity = new Price
        {
            CompanyId = model.CompanyId,
            SourceType = model.SourceType,
            Date = model.Date,
            Value = model.Value
        };

        var message = $"price of '{model.CompanyId}' update at {model.Date:yyyy MMMM dd}";

        var (error, _) = await priceRepository.UpdateAsync(ctxEntity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateTime date)
    {
        companyId = companyId.ToUpperInvariant().Trim();

        var message = $"price of '{companyId}' delete at {date:yyyy MMMM dd}";
        var (error, deletedEntity) = await priceRepository.DeleteAsync(message, companyId, date);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
}