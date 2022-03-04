﻿using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.RabbitServices;
using IM.Service.Common.Net.RabbitServices.Configuration;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Data.Domain.DataAccess;
using IM.Service.Data.Domain.DataAccess.Filters;
using IM.Service.Data.Domain.Entities;
using IM.Service.Data.Models.Api.Http;
using IM.Service.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IM.Service.Data.Services.RestApi;

public class SplitApi
{
    private readonly Repository<Company> companyRepository;
    private readonly Repository<Split> stockSplitRepository;
    private readonly string rabbitConnectionString;

    public SplitApi(
        IOptions<ServiceSettings> options,
        Repository<Company> companyRepository,
        Repository<Split> stockSplitRepository)
    {
        rabbitConnectionString = options.Value.ConnectionStrings.Mq;

        this.companyRepository = companyRepository;
        this.stockSplitRepository = stockSplitRepository;
    }

    public async Task<ResponseModel<SplitGetDto>> GetAsync(string companyId, DateTime date)
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
                    SourceId = stockSplit.SourceId,
                    Value = stockSplit.Value,
                }
            };

        return new()
        {
            Errors = new[] { $"Split for '{companyId}' not found" }
        };
    }
    public async Task<ResponseModel<PaginatedModel<SplitGetDto>>> GetAsync(CompanySourceDateFilter<Split> filter, HttpPagination pagination)
    {
        var filteredQuery = stockSplitRepository.GetQuery(filter.FilterExpression);
        var count = await stockSplitRepository.GetCountAsync(filteredQuery);
        var paginatedQuery = stockSplitRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

        var result = await paginatedQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new SplitGetDto
            {
                Company = y.Name,
                Date = x.Date,
                Value = x.Value,
                SourceId = x.SourceId
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
    public async Task<ResponseModel<PaginatedModel<SplitGetDto>>> GetLastAsync(CompanySourceDateFilter<Split> filter, HttpPagination pagination)
    {
        var filteredQuery = stockSplitRepository.GetQuery(filter.FilterExpression);

        var queryResult = await filteredQuery
            .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new SplitGetDto
            {
                Company = y.Name,
                Date = x.Date,
                Value = x.Value,
                SourceId = x.SourceId
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

    public async Task<ResponseModel<string>> CreateAsync(SplitPostDto model)
    {
        var entity = new Split
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            SourceId = model.SourceId,
            Date = model.Date,
            Value = model.Value
        };

        var message = $"Split of '{entity.CompanyId}' create at {entity.Date:yyyy MMMM dd}";

        var (error, _) = await stockSplitRepository.CreateAsync(entity, message);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = message + " success" };
    }
    public async Task<ResponseModel<string>> CreateAsync(IEnumerable<SplitPostDto> models)
    {
        var Splits = models.ToArray();

        if (!Splits.Any())
            return new() { Errors = new[] { "Split data for creating not found" } };

        var entities = Splits.Select(x => new Split
        {
            CompanyId = string.Intern(x.CompanyId.Trim().ToUpperInvariant()),
            SourceId = x.SourceId,
            Date = x.Date,
            Value = x.Value
        })
        .ToArray();

        var (error, result) = await stockSplitRepository.CreateAsync(entities, new CompanyDateComparer<Split>(), $"Source count: {entities.Length}");

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = $"Stock splits count: {result.Length} was successed" };
    }
    public async Task<ResponseModel<string>> UpdateAsync(SplitPostDto model)
    {
        var entity = new Split
        {
            CompanyId = string.Intern(model.CompanyId.Trim().ToUpperInvariant()),
            SourceId = model.SourceId,
            Date = model.Date,
            Value = model.Value
        };

        var info = $"Split of '{entity.CompanyId}' update at {entity.Date:yyyy MMMM dd}";

        var (error, _) = await stockSplitRepository.UpdateAsync(new object[] { entity.CompanyId, entity.Date }, entity, info);

        return error is not null
            ? new() { Errors = new[] { error } }
            : new() { Data = info + " success" };
    }
    public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateOnly date)
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
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Splits, QueueActions.Call, DateTime.UtcNow.ToShortDateString());
        return "Load stock splits is running...";
    }
    public string Load(string companyId)
    {
        companyId = companyId.Trim().ToUpperInvariant();
        var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
        publisher.PublishTask(QueueNames.MarketData, QueueEntities.Split, QueueActions.Call, companyId);
        return $"Load stock splits for '{companyId}' is running...";
    }
}