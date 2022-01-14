using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.CompanyServices;
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
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Company.Data.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Data.Services.DtoServices;

public class StockSplitsDtoManager
{
    private readonly Repository<DataAccess.Entities.Company> companyRepository;
    private readonly Repository<StockSplit> stockSplitRepository;
    private readonly string rabbitConnectionString;

    public StockSplitsDtoManager(
        IOptions<ServiceSettings> options,
        Repository<DataAccess.Entities.Company> companyRepository,
        Repository<StockSplit> stockSplitRepository)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        this.companyRepository = companyRepository;
        this.stockSplitRepository = stockSplitRepository;
    }

    public async Task<ResponseModel<StockSplitGetDto>> GetAsync(string companyId, DateTime date)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var company = await companyRepository.FindAsync(companyId);

        if (company is null)
            return new() { Errors = new[] { $"'{companyId}' not found" } };

        var stockSplit = await stockSplitRepository.FindAsync(company.Id, date);

        if (stockSplit is not null)
            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Company = company.Name,
                    Date = stockSplit.Date,
                    SourceType = stockSplit.SourceType,
                    Value = stockSplit.Value,
                }
            };

        return new()
        {
            Errors = new[] { $"Split for '{companyId}' not found" }
        };
    }
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetAsync(CompanyDataFilterByDate<StockSplit> filter, HttpPagination pagination)
    {
        var filteredQuery = stockSplitRepository.GetQuery(filter.FilterExpression);
        var count = await stockSplitRepository.GetCountAsync(filteredQuery);
        var paginatedQuery = stockSplitRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

        var result = await paginatedQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new StockSplitGetDto
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
    public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLastAsync(CompanyDataFilterByDate<StockSplit> filter, HttpPagination pagination)
    {
        var filteredQuery = stockSplitRepository.GetQuery(filter.FilterExpression);

        var queryResult = await filteredQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new StockSplitGetDto
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

    public async Task<ResponseModel<string>> CreateAsync(StockSplitPostDto model)
    {
        var entity = new StockSplit
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            SourceType = model.SourceType,
            Date = model.Date.Date,
            Value = model.Value
        };

        var message = $"Split of '{entity.CompanyId}' create at {entity.Date:yyyy MMMM dd}";

        var (error, _) = await stockSplitRepository.CreateAsync(entity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<StockSplitPostDto> models)
    {
        var stockSplits = models.ToArray();

        if (!stockSplits.Any())
            return new() { Errors = new[] { "Split data for creating not found" } };

        var entities = stockSplits.Select(x => new StockSplit
        {
            CompanyId = string.Intern(x.CompanyId.Trim().ToUpperInvariant()),
            SourceType = x.SourceType,
            Date = x.Date,
            Value = x.Value
        });

        var (error, result) = await stockSplitRepository.CreateAsync(entities, new CompanyDateComparer<StockSplit>(), "Stock splits");

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = $"Stock splits count: {result.Length} was successed" };
    }
    public async Task<ResponseModel<string>> UpdateAsync(StockSplitPostDto model)
    {
        var entity = new StockSplit
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            SourceType = model.SourceType,
            Date = model.Date,
            Value = model.Value
        };

        var info = $"Split of '{entity.CompanyId}' update at {entity.Date:yyyy MMMM dd}";

        var (error, _) = await stockSplitRepository.UpdateAsync(new object[] { entity.CompanyId, entity.Date }, entity, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateTime date)
    {
        companyId = string.Intern(companyId.Trim().ToUpperInvariant());

        var info = $"Split of '{companyId}' delete at {date:yyyy MMMM dd}";

        var (error, _) = await stockSplitRepository.DeleteByIdAsync(new object[] { companyId, date }, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }

    public string Load()
    {
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.CompanyData, QueueEntities.StockSplits, QueueActions.Call, DateTime.UtcNow.ToShortDateString());
        return "Load stock splits is running.";
    }
    public string Load(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.CompanyData, QueueEntities.StockSplit, QueueActions.Call, companyId);
        return $"Load stock splits for '{companyId}' is running.";
    }
}