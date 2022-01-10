using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Common.Net.RepositoryService.Filters;
using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Company.Data.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Data.Services.DtoServices;

public class PricesDtoManager
{
    private readonly Repository<Price> priceRepository;
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<StockSplit> stockSplitRepository;
    private readonly Repository<StockVolume> stockVolumeRepository;
    private readonly string rabbitConnectionString;

    public PricesDtoManager(
        IOptions<ServiceSettings> options,
        Repository<Price> priceRepository,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<StockSplit> stockSplitRepository,
        Repository<StockVolume> stockVolumeRepository)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        this.priceRepository = priceRepository;
        this.companyRepository = companyRepository;
        this.stockSplitRepository = stockSplitRepository;
        this.stockVolumeRepository = stockVolumeRepository;
    }

    public async Task<ResponseModel<PriceGetDto>> GetAsync(string companyId, DateTime date)
    {
        companyId = companyId.ToUpperInvariant().Trim();
        var company = await companyRepository.FindAsync(companyId);

        if (company is null)
            return new() { Errors = new[] { $"'{companyId}' not found" } };

        var price = await priceRepository.FindAsync(company.Id, date);

        if (price is null)
            return new() { Errors = new[] { $"Price for '{companyId}' not found" } };

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

        if (queryResult.Any())
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
                var targetStockVolumes = stockVolumesDictionary.ContainsKey(groupSplits.Key)
                    ? stockVolumesDictionary[groupSplits.Key].OrderBy(x => x.Date)
                    : null;

                var groupedSplits = groupSplits.OrderByDescending(x => x.Date).ToArray();

                foreach (var split in groupedSplits)
                {
                    var priceSplitResult = priceResult.Where(x => x.CompanyId == groupSplits.Key && x.Date >= split.Date).ToArray();
                    var stockSplitValue = groupedSplits.Where(x => x.Date <= split.Date).Aggregate(1, (current, x) => current * x.Value);

                    result.AddRange(priceSplitResult.Select(x => new PriceGetDto
                    {
                        Ticker = groupSplits.Key,
                        Company = companyName,
                        StockVolume = targetStockVolumes?.LastOrDefault(y => y.Date <= x.Date)?.Value,
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
                var targetStockVolumes = stockVolumesDictionary.ContainsKey(group.Key)
                    ? stockVolumesDictionary[group.Key].OrderBy(x => x.Date)
                    : null;

                result.AddRange(group.Select(x => new PriceGetDto
                {
                    Ticker = group.Key,
                    Company = companyName,
                    StockVolume = targetStockVolumes?.LastOrDefault(y => y.Date <= x.Date)?.Value,
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
                Items = result.OrderBy(x => x.Date).ToArray(),
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
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Company)
            .ToArray();

        var paginatedResult = pagination.GetPaginatedResult(groupedResult);

        var result = new List<PriceGetDto>(paginatedResult.Length);

        if (paginatedResult.Any())
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
                var targetStockVolumes = stockVolumesDictionary.ContainsKey(groupSplits.Key)
                    ? stockVolumesDictionary[groupSplits.Key].OrderBy(x => x.Date)
                    : null;
                var groupedSplits = groupSplits.OrderByDescending(x => x.Date).ToArray();

                foreach (var split in groupedSplits)
                {
                    var priceSplitResult = priceResult.Where(x => x.CompanyId == groupSplits.Key && x.Date >= split.Date).ToArray();
                    var stockSplitValue = groupedSplits.Aggregate(1, (current, x) => current * x.Value);

                    result.AddRange(priceSplitResult.Select(x => new PriceGetDto
                    {
                        Ticker = groupSplits.Key,
                        Company = companyName,
                        StockVolume = targetStockVolumes?.LastOrDefault(y => y.Date <= x.Date)?.Value,
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
                StockVolume = stockVolumesDictionary.ContainsKey(x.CompanyId)
                    ? stockVolumesDictionary[x.CompanyId].OrderBy(y => y.Date).LastOrDefault(y => y.Date <= x.Date)?.Value
                    : null,
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
                Items = result.OrderByDescending(x => x.Date).ThenBy(x => x.Company).ToArray(),
                Count = groupedResult.Length
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
        var message = $"Price of '{model.CompanyId}' create at {model.Date:yyyy MMMM dd}";
        var (error, _) = await priceRepository.CreateAsync(ctxEntity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<PricePostDto> models)
    {
        var prices = models.ToArray();

        if (!prices.Any())
            return new() { Errors = new[] { "Price data for creating not found" } };

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
            : new() { Data = $"Prices count: {result.Length} was successed" };
    }
    public async Task<ResponseModel<string>> UpdateAsync(PricePostDto model)
    {
        var entity = new Price
        {
            CompanyId = model.CompanyId,
            SourceType = model.SourceType,
            Date = model.Date,
            Value = model.Value
        };

        var info = $"Price of '{model.CompanyId}' update at {model.Date:yyyy MMMM dd}";

        var (error, _) = await priceRepository.UpdateAsync(new object[] { entity.CompanyId, entity.Date }, entity, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateTime date)
    {
        companyId = companyId.ToUpperInvariant().Trim();

        var info = $"Price of '{companyId}' delete at {date:yyyy MMMM dd}";
        var (error, _) = await priceRepository.DeleteByIdAsync(new object[] { companyId, date }, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }

    public string Load()
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.CompanyData,QueueEntities.Prices,QueueActions.Call, DateTime.UtcNow.ToShortDateString());
        return "Load prices is running.";
    }
}