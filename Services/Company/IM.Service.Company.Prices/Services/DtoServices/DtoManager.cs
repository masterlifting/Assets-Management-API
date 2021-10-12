using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;
using CommonServices.RabbitServices;

using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.DataAccess.Repository;
using IM.Service.Company.Prices.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Service.Company.Prices.Services.DtoServices
{
    public class DtoManager
    {
        private readonly RepositorySet<Price> repository;
        private readonly string rabbitConnectionString;
        public DtoManager(RepositorySet<Price> repository, IOptions<ServiceSettings> options)
        {
            this.repository = repository;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<ResponseModel<PriceGetDto>> GetAsync(PriceIdentity identity)
        {
            var tickerName = identity.TickerName.ToUpperInvariant().Trim();

            var result = await repository.FindAsync(tickerName, identity.Date);

            if (result is null)
                return new() { Errors = new[] { "price not found" } };

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    TickerName = result.TickerName,
                    Date = result.Date,
                    Value = result.Value,
                    SourceType = result.SourceType
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetAsync(HttpFilter filter, HttpPagination pagination)
        {
            var errors = Array.Empty<string>();

            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);
            var count = await repository.GetCountAsync(filteredQuery);
            var paginatedQuery = repository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

            var result = await paginatedQuery.Select(x => new PriceGetDto
            {
                TickerName = x.TickerName,
                Date = x.Date,
                Value = x.Value,
                SourceType = x.SourceType
            })
                .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = result,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetLastAsync(HttpFilter filter, HttpPagination pagination)
        {
            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);

            var queryResult = await filteredQuery
                .OrderBy(x => x.Date)
                .Select(x => new PriceGetDto
                {
                    TickerName = x.TickerName,
                    Date = x.Date,
                    Value = x.Value,
                    SourceType = x.SourceType
                })
                .ToArrayAsync();

            var groupedResult = queryResult
                .GroupBy(x => x.TickerName)
                .Select(x => x.Last())
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
        public async Task<ResponseModel<string>> CreateAsync(PricePostDto model)
        {
            var ctxEntity = new Price
            {
                TickerName = model.TickerName,
                SourceType = model.SourceType,
                Date = model.Date.Date,
                Value = model.Value
            };
            var message = $"price for: '{model.TickerName}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, price) = await repository.CreateAsync(ctxEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = price!.TickerName,
                    Date = price.Date,
                    SourceType = price.SourceType
                }));

            return new() { Data = message + " created" };
        }
        public async Task<ResponseModel<string>> CreateAsync(IEnumerable<PricePostDto> models)
        {
            var prices = models.ToArray();

            if (!prices.Any())
                return new() { Errors = new[] { "price data to creating not found" } };

            var ctxEntities = prices.GroupBy(x => x.Date.Date).Select(x => new Price
            {
                TickerName = x.Last().TickerName,
                SourceType = x.Last().SourceType,
                Date = x.Last().Date.Date,
                Value = x.Last().Value
            });

            var message = $"prices for: '{prices[0].TickerName}'";
            var (errors, _) = await repository.CreateAsync(ctxEntities, new PriceComparer(), message);

            return errors.Any()
                ? new() { Errors = errors }
                : new() { Data = message + " created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(PricePostDto model)
        {
            var ctxEntity = new Price
            {
                TickerName = model.TickerName,
                SourceType = model.SourceType,
                Date = model.Date,
                Value = model.Value
            };

            var message = $"price for: '{model.TickerName}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, price) = await repository.UpdateAsync(ctxEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = price!.TickerName,
                    Date = price.Date,
                    SourceType = price.SourceType
                }));

            return new() { Data = message + " updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(PriceIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"price for: '{ticker}' of date: {identity.Date:yyyy MMMM dd}";
            var errors = await repository.DeleteAsync(message, ticker, identity.Date);

            if (errors.Any())
                return new() { Errors = errors };

            //set to queue
            var previousPrices = await repository.GetAnyAsync(x => x.TickerName == identity.TickerName);

            if (!previousPrices)
                return new() { Data = message + " deleted. But not set in analyzer!" };

            Price? previousPrice = null;
            var previousDate = identity.Date;

            while (previousPrice is null)
            {
                previousDate = previousDate.AddDays(-1);
                previousPrice = await repository.FindAsync(identity.TickerName, previousDate);
            }

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = previousPrice.TickerName,
                    Date = previousPrice.Date,
                    SourceType = previousPrice.SourceType
                }));

            return new() { Data = message + " deleted" };
        }
    }
}
