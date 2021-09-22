
using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.GatewayCompanies;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;
using CommonServices.RabbitServices;

using IM.Gateway.Companies.Clients;
using IM.Gateway.Companies.DataAccess.Entities;
using IM.Gateway.Companies.DataAccess.Repository;
using IM.Gateway.Companies.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class DtoStockSplitManager
    {
        private readonly RepositorySet<StockSplit> repository;
        private readonly PricesClient pricesClient;
        private readonly string rabbitConnectionString;
        public DtoStockSplitManager(
            RepositorySet<StockSplit> repository
            , IOptions<ServiceSettings> options
            , PricesClient pricesClient)
        {
            this.repository = repository;
            this.pricesClient = pricesClient;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<ResponseModel<StockSplitGetDto>> GetAsync(PriceIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();

            var result = await repository.FindAsync(ticker, identity.Date);

            if (result is null)
                return new() { Errors = new[] { "split not found" } };

            var company = await repository.GetDbSetBy<Company>().FindAsync(ticker);

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    TickerName = result.CompanyTicker,
                    Date = result.Date,
                    Divider = result.Divider,
                    Company = company.Name
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetAsync(HttpStockSplitFilter filter, HttpPagination pagination)
        {
            var errors = Array.Empty<string>();

            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);
            var count = await repository.GetCountAsync(filteredQuery);
            var paginatedQuery = repository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

            var companies = repository.GetDbSetBy<Company>();

            var result = await paginatedQuery.Join(companies, x => x.CompanyTicker, y => y.Ticker, (x, y) => new StockSplitGetDto
            {
                Company = y.Name,
                TickerName = x.CompanyTicker,
                Divider = x.Divider,
                Date = x.Date
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
        public async Task<ResponseModel<PaginatedModel<StockSplitGetDto>>> GetLastAsync(HttpStockSplitFilter filter, HttpPagination pagination)
        {
            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);

            var companies = repository.GetDbSetBy<Company>();

            var queryResult = await filteredQuery
                .OrderBy(x => x.Date)
                .Join(companies, x => x.CompanyTicker, y => y.Ticker, (x, y) => new StockSplitGetDto
                {
                    Company = y.Name,
                    TickerName = x.CompanyTicker,
                    Divider = x.Divider,
                    Date = x.Date
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

        public async Task<ResponseModel<string>> CreateAsync(StockSplitPostDto model)
        {
            var ticker = model.TickerName.ToUpperInvariant().Trim();
            var multiplier = 1;

            var previous = await repository.GetSampleAsync(x => x.CompanyTicker == ticker);
            if (previous.Length > 1)
                multiplier = previous.Last().Divider;

            var ctxEntity = new StockSplit()
            {
                CompanyTicker = ticker,
                Date = model.Date,
                Divider = model.Divider * multiplier
            };
            var message = $"stock split for: '{ticker}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, stockSplit) = await repository.CreateAsync(ctxEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            var priceGetResponse = await pricesClient.Get<PriceGetDto>("prices", stockSplit!.CompanyTicker, stockSplit.Date.Year, stockSplit.Date.Month, stockSplit.Date.Day);

            if (priceGetResponse.Errors.Any())
                return new() { Data = message + " created. But not set in analyzer!" };

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.GetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = priceGetResponse.Data!.TickerName,
                    Date = priceGetResponse.Data.Date,
                    SourceType = priceGetResponse.Data.SourceType
                }));

            return new() { Data = message + " created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(StockSplitPostDto model)
        {
            var ticker = model.TickerName.ToUpperInvariant().Trim();
            var multiplier = 1;

            var previous = await repository.GetSampleAsync(x => x.CompanyTicker == ticker);
            if (previous.Length > 1)
                multiplier = previous.Last().Divider;

            var ctxEntity = new StockSplit
            {
                CompanyTicker = ticker,
                Date = model.Date,
                Divider = model.Divider * multiplier
            };

            var message = $"stock split for: '{ticker}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, stockSplit) = await repository.UpdateAsync(ctxEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            var priceGetResponse = await pricesClient.Get<PriceGetDto>("prices", stockSplit!.CompanyTicker, stockSplit.Date.Year, stockSplit.Date.Month, stockSplit.Date.Day);

            if (priceGetResponse.Errors.Any())
                return new() { Data = message + " updated. But not set in analyzer!" };

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.GetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = priceGetResponse.Data!.TickerName,
                    Date = priceGetResponse.Data.Date,
                    SourceType = priceGetResponse.Data.SourceType
                }));

            return new() { Data = message + " updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(PriceIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"stock split for: '{ticker}' of date: {identity.Date:yyyy MMMM dd}";
            var errors = await repository.DeleteAsync(message, ticker, identity.Date);

            if (errors.Any())
                return new() { Errors = errors };

            var priceGetResponse = await pricesClient.Get<PriceGetDto>("prices", identity.TickerName, identity.Date.Year, identity.Date.Month, identity.Date.Day);

            if (priceGetResponse.Errors.Any())
                return new() { Data = message + " deleted. But not set in analyzer!" };

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.GetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = priceGetResponse.Data!.TickerName,
                    Date = priceGetResponse.Data.Date,
                    SourceType = priceGetResponse.Data.SourceType
                }));

            return new() { Data = message + " deleted" };
        }
    }
}
