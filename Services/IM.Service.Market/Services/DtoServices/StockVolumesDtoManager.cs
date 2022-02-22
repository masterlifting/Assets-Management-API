using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
using IM.Service.Common.Net.RepositoryService.Filters;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Market.DataAccess.Entities;
using IM.Service.Market.DataAccess.Repositories;
using IM.Service.Market.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Market.Services.DtoServices;

public class StockVolumesDtoManager
{
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<StockVolume> stockVolumeRepository;
    private readonly string rabbitConnectionString;

    public StockVolumesDtoManager(
        IOptions<ServiceSettings> options,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<StockVolume> stockVolumeRepository)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        this.companyRepository = companyRepository;
        this.stockVolumeRepository = stockVolumeRepository;
    }

    public async Task<ResponseModel<StockVolumeGetDto>> GetAsync(string companyId, DateTime date)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var company = await companyRepository.FindAsync(companyId);

        if (company is null)
            return new() { Errors = new[] { $"'{companyId}' not found" } };

        var stockVolume = await stockVolumeRepository.FindAsync(company.Id, date);

        if (stockVolume is not null)
            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Company = company.Name,
                    Date = stockVolume.Date,
                    SourceType = stockVolume.SourceType,
                    Value = stockVolume.Value,
                }
            };

        return new()
        {
            Errors = new[] { $"Stock volume for '{companyId}' not found" }
        };
    }
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetAsync(CompanyDataFilterByDate<StockVolume> filter, HttpPagination pagination)
    {
        var filteredQuery = stockVolumeRepository.GetQuery(filter.FilterExpression);
        var count = await stockVolumeRepository.GetCountAsync(filteredQuery);
        var paginatedQuery = stockVolumeRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

        var result = await paginatedQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new StockVolumeGetDto
            {
                Company = y.Name,
                Date = x.Date,
                Value = x.Value,
                SourceType = x.SourceType
            })
            .ToArrayAsync();

        return new()
        {
            Errors = Array.Empty<string>(),
            Data = new()
            {
                Items = result,
                Count = count
            }
        };
    }
    public async Task<ResponseModel<PaginatedModel<StockVolumeGetDto>>> GetLastAsync(CompanyDataFilterByDate<StockVolume> filter, HttpPagination pagination)
    {
        var filteredQuery = stockVolumeRepository.GetQuery(filter.FilterExpression);

        var queryResult = await filteredQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new StockVolumeGetDto
            {
                Company = y.Name,
                Date = x.Date,
                Value = x.Value,
                SourceType = x.SourceType
            })
            .ToArrayAsync();

        var groupedResult = queryResult
            .GroupBy(x => x.Company)
            .Select(x => x
                .OrderBy(y => y.Date)
                .Last())
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.Company)
            .ToArray();

        return new()
        {
            Errors = Array.Empty<string>(),
            Data = new()
            {
                Items = pagination.GetPaginatedResult(groupedResult),
                Count = groupedResult.Length
            }
        };
    }

    public async Task<ResponseModel<string>> CreateAsync(StockVolumePostDto model)
    {
        var entity = new StockVolume
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            SourceType = model.SourceType,
            Date = model.Date,
            Value = model.Value
        };
        var message = $"Stock volume of '{entity.CompanyId}' create at {entity.Date:yyyy MMMM dd}";
        var (error, _) = await stockVolumeRepository.CreateAsync(entity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<StockVolumePostDto> models)
    {
        var entities = models.ToArray();

        if (!entities.Any())
            return new() { Errors = new[] { "Stock volume data for creating not found" } };

        var ctxEntities = entities.Select(x => new StockVolume
        {
            CompanyId = string.Intern(x.CompanyId.Trim().ToUpperInvariant()),
            SourceType = x.SourceType,
            Date = x.Date,
            Value = x.Value
        })
        .ToArray();

        var (error, result) = await stockVolumeRepository.CreateAsync(ctxEntities, new CompanyDateComparer<StockVolume>(), $"Source count: {entities.Length}");

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = $"Stock volumes count: {result.Length} was successed" };
    }
    public async Task<ResponseModel<string>> UpdateAsync(StockVolumePostDto model)
    {
        var entity = new StockVolume
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            SourceType = model.SourceType,
            Date = model.Date,
            Value = model.Value
        };

        var info = $"Stock volume of '{entity.CompanyId}' update at {entity.Date:yyyy MMMM dd}";
        var (error, _) = await stockVolumeRepository.UpdateAsync(new object[] { entity.CompanyId, entity.Date }, entity, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateOnly date)
    {
        companyId = string.Intern(companyId.Trim().ToUpperInvariant());

        var info = $"Stock volume of '{companyId}' delete at {date:yyyy MMMM dd}";
        var (error, _) = await stockVolumeRepository.DeleteByIdAsync(new object[] { companyId, date }, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }

    public string Load()
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.StockVolumes, QueueActions.Call, DateTime.UtcNow.ToShortDateString());
        return "Load stock volumes is running...";
    }
    public string Load(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.StockVolume, QueueActions.Call, companyId);
        return $"Load stock volumes for '{companyId}' is running...";
    }
}