using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;

using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Service.Company.Prices.Services.DtoServices
{
    public class DtoManager
    {
        private readonly RepositorySet<Price> repository;
        public DtoManager(RepositorySet<Price> repository) => this.repository = repository;

        public async Task<ResponseModel<PriceGetDto>> GetAsync(PriceIdentity identity)
        {
            var tickerName = identity.TickerName.ToUpperInvariant().Trim();

            var result = await repository.FindAsync(tickerName, identity.Date);

            if (result is null)
                return new() { Errors = new[] { "price not found" } };

            var ticker = await repository.GetDbSetBy<Ticker>().FindAsync(tickerName);

            var sourceType = await repository.GetDbSetBy<SourceType>().FindAsync(ticker.SourceTypeId);

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    TickerName = result.TickerName,
                    Date = result.Date,
                    Value = result.Value,
                    SourceType = sourceType.Name
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<PriceGetDto>>> GetAsync(HttpFilter filter, HttpPagination pagination)
        {
            var errors = Array.Empty<string>();

            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);
            var count = await repository.GetCountAsync(filteredQuery);
            var paginatedQuery = repository.GetPaginationQuery(filteredQuery, pagination, x => x.Date);

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var result = await paginatedQuery
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new PriceGetDto
                {
                    TickerName = x.Price.TickerName,
                    Date = x.Price.Date,
                    Value = x.Price.Value,
                    SourceType = y.Name
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

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var queryResult = await filteredQuery
                .OrderBy(x => x.Date)
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new PriceGetDto
                {
                    TickerName = x.Price.TickerName,
                    Date = x.Price.Date,
                    Value = x.Price.Value,
                    SourceType = y.Name
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
                Date = model.Date,
                Value = model.Value
            };
            var message = $"price for: '{model.TickerName}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, _) = await repository.CreateAsync(ctxEntity, message);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(PricePostDto model)
        {
            var ctxEntity = new Price
            {
                TickerName = model.TickerName,
                Date = model.Date,
                Value = model.Value
            };

            var message = $"price for: '{model.TickerName}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, _) = await repository.UpdateAsync(ctxEntity, message);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(PriceIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"price for: '{ticker}' of date: {identity.Date:yyyy MMMM dd}";
            var errors = await repository.DeleteAsync(message, ticker, identity.Date);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "deleted" };
        }
    }
}
