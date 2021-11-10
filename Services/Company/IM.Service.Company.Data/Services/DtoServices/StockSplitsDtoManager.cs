using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;

using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Common.Net.RepositoryService.Filters;

namespace IM.Service.Company.Data.Services.DtoServices
{
    public class StockSplitsDtoManager
    {
        private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
        private readonly RepositorySet<StockSplit> stockSplitRepository;
        private readonly string rabbitConnectionString;
        public StockSplitsDtoManager(
            RepositorySet<DataAccess.Entities.Company> companyRepository,
            RepositorySet<StockSplit> stockSplitRepository,
            IOptions<ServiceSettings> options)
        {
            this.companyRepository = companyRepository;
            this.stockSplitRepository = stockSplitRepository;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<ResponseModel<StockSplitGetDto>> GetAsync(string companyId, DateTime date)
        {
            var company = await companyRepository.FindAsync(companyId.ToUpperInvariant().Trim());

            if (company is null)
                return new() { Errors = new[] { "company not found" } };

            var stockSplit = await stockSplitRepository.FindAsync(company.Id, date);

            if (stockSplit is null)
                return new() { Errors = new[] { "split not found" } };

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
            var ctxEntity = new StockSplit
            {
                CompanyId = model.CompanyId,
                SourceType = model.SourceType,
                Date = model.Date.Date,
                Value = model.Value
            };
            var message = $"split of '{model.CompanyId}' create at {model.Date:yyyy MMMM dd}";
            var (error, stockSplit) = await stockSplitRepository.CreateAsync(ctxEntity, message);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Call
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = stockSplit!.CompanyId,
                    Date = stockSplit.Date,
                }));

            return new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> CreateAsync(IEnumerable<StockSplitPostDto> models)
        {
            var stockSplits = models.ToArray();

            if (!stockSplits.Any())
                return new() { Errors = new[] { "split data for creating not found" } };

            var ctxEntities = stockSplits.GroupBy(x => x.Date.Date).Select(x => new StockSplit
            {
                CompanyId = x.Last().CompanyId,
                SourceType = x.Last().SourceType,
                Date = x.Last().Date.Date,
                Value = x.Last().Value
            });

            var message = $"splits of '{stockSplits[0].CompanyId}' create";
            var (error, _) = await stockSplitRepository.CreateAsync(ctxEntities, new CompanyDateComparer<StockSplit>(), message);

            return error is not null
                ? new() { Errors = new[] { error } }
                : new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(StockSplitPostDto model)
        {
            var ctxEntity = new StockSplit
            {
                CompanyId = model.CompanyId,
                SourceType = model.SourceType,
                Date = model.Date,
                Value = model.Value
            };

            var message = $"split of '{model.CompanyId}' update at {model.Date:yyyy MMMM dd}";
            var (error, price) = await stockSplitRepository.UpdateAsync(ctxEntity, message);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Call
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = price!.CompanyId,
                    Date = price.Date,
                }));

            return new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(string companyId, DateTime date)
        {
            companyId = companyId.ToUpperInvariant().Trim();

            var message = $"split of '{companyId}' delete at {date:yyyy MMMM dd}";
            var (error, deletedEntity) = await stockSplitRepository.DeleteAsync(message, companyId, date);

            if (error is not null)
                return new() { Errors = new[] { error } };

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.Call
                , JsonSerializer.Serialize(new PriceIdentityDto
                {
                    CompanyId = deletedEntity!.CompanyId,
                    Date = deletedEntity.Date,
                }));

            return new() { Data = message + " success" };
        }
    }
}
