
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

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;

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
                    Value = result.Value,
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
                Value = x.Value,
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
                    Value = x.Value,
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

            var newEntity = new StockSplit
            {
                CompanyTicker = ticker,
                Date = model.Date,
                Value = model.Value,
                Divider = model.Value
            };

            var ctxStockSplits = await repository.GetSampleAsync(x => x.CompanyTicker.Equals(ticker));

            if (ctxStockSplits.Any())
            {
                var previous = ctxStockSplits.Where(x => x.Date < model.Date).ToArray();

                if (previous.Any())
                    newEntity.Divider = newEntity.Value * previous[^1].Divider;

                var toUpdate = ctxStockSplits.Where(x => x.Date > model.Date).ToArray();

                if (toUpdate.Any())
                {
                    toUpdate[0].Divider = toUpdate[0].Value * newEntity.Divider;

                    for (var i = 1; i < toUpdate.Length; i++)
                        toUpdate[i].Divider = toUpdate[i].Value * toUpdate[i - 1].Divider;

                    await repository.UpdateAsync(toUpdate, $"stock split for: '{ticker}' of date more: {model.Date:yyyy MMMM dd}");
                }
            }

            var message = $"stock split for: '{ticker}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, stockSplit) = await repository.CreateAsync(newEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            var queryString = GetQueryString(HttpRequestFilterType.More, stockSplit!.CompanyTicker, stockSplit.Date.Year, stockSplit.Date.Month, stockSplit.Date.Day);
            var priceDtoResponse = await pricesClient.Get<PriceGetDto>("prices", queryString, new HttpPagination(1, 10));

            if (priceDtoResponse.Errors.Any())
                return new() { Data = message + " created. But not set in analyzer!" };

            var priceDto = priceDtoResponse.Data!.Items[0];

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = priceDto.TickerName,
                    Date = priceDto.Date,
                    SourceType = priceDto.SourceType
                }));

            return new() { Data = message + " created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(StockSplitPostDto model)
        {
            var ticker = model.TickerName.ToUpperInvariant().Trim();

            var ctxStockSplits = await repository.GetSampleAsync(x => x.CompanyTicker.Equals(ticker));

            if (ctxStockSplits.Length > 1)
            {
                var toUpdate = ctxStockSplits.Where(x => x.Date >= model.Date).ToArray();
                
                if (toUpdate.Any())
                {
                    toUpdate[0].Value = model.Value;
                    toUpdate[0].Divider = model.Value;

                    var previous = ctxStockSplits.Where(x => x.Date < model.Date).ToArray();

                    if (previous.Any())
                        toUpdate[0].Divider = toUpdate[0].Value * previous[^1].Divider;

                    for (var i = 1; i < toUpdate.Length; i++)
                        toUpdate[i].Divider = toUpdate[i].Value * toUpdate[i - 1].Divider;

                    ctxStockSplits = toUpdate;
                }
            }
            else
            {
                ctxStockSplits[0].Value = model.Value;
                ctxStockSplits[0].Divider = model.Value;
            }

            var message = $"stock split for: '{ticker}' of date more and equal: {model.Date:yyyy MMMM dd}";
            var (errors, stockSplits) = await repository.UpdateAsync(ctxStockSplits, message);

            if (errors.Any())
                return new() { Errors = errors };

            var priceDtoResponse = await pricesClient.Get<PriceGetDto>("prices", stockSplits[0].CompanyTicker, stockSplits[0].Date.Year, stockSplits[0].Date.Month, stockSplits[0].Date.Day);

            if (priceDtoResponse.Errors.Any())
                return new() { Data = message + " updated. But not set in analyzer!" };

            var priceDto = priceDtoResponse.Data!;

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = priceDto.TickerName,
                    Date = priceDto.Date,
                    SourceType = priceDto.SourceType
                }));

            return new() { Data = message + " updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(PriceIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"stock split for: '{ticker}' of date: {identity.Date:yyyy MMMM dd}";

            var ctxStockSplits = await repository.GetSampleAsync(x => x.CompanyTicker.Equals(ticker));

            if (ctxStockSplits.Length > 1)
            {
                var toUpdate = ctxStockSplits.Where(x => x.Date > identity.Date).ToArray();

                if (toUpdate.Any())
                {
                    var previous = ctxStockSplits.Where(x => x.Date < identity.Date).ToArray();

                    toUpdate[0].Divider = previous.Any() 
                        ? toUpdate[0].Value * previous[^1].Divider 
                        : toUpdate[0].Value;

                    for (var i = 1; i < toUpdate.Length; i++)
                        toUpdate[i].Divider = toUpdate[i].Value * toUpdate[i - 1].Divider;

                    await repository.UpdateAsync(toUpdate, $"stock split for: '{ticker}' of date more: {identity.Date:yyyy MMMM dd}");
                }
            }

            var errors = await repository.DeleteAsync(message, ticker, identity.Date);

            if (errors.Any())
                return new() { Errors = errors };

            var queryString = GetQueryString(HttpRequestFilterType.More, ticker, identity.Date.Year, identity.Date.Month, identity.Date.Day);
            var priceDtoResponse = await pricesClient.Get<PriceGetDto>("prices", queryString, new HttpPagination(1, 10));

            if (priceDtoResponse.Errors.Any())
                return new() { Data = message + " deleted. But not set in analyzer!" };

            var priceDto = priceDtoResponse.Data!.Items[0];

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Price
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new PriceGetDto
                {
                    TickerName = priceDto.TickerName,
                    Date = priceDto.Date,
                    SourceType = priceDto.SourceType
                }));

            return new() { Data = message + " deleted" };
        }
    }
}
