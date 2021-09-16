using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using System;
using System.Threading.Tasks;
using System.Linq;
using IM.Service.Company.Prices.DataAccess.Entities;
using IM.Service.Company.Prices.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;

namespace IM.Service.Company.Prices.Services.DtoServices
{
    public class PriceDtoAggregator
    {
        private readonly RepositorySet<Price> repository;
        public PriceDtoAggregator(RepositorySet<Price> repository) => this.repository = repository;

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetAsync(FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var dateFilteredQuery = repository.QueryFilter(filter.FilterDateExpression<Price>());

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var queryResult = await dateFilteredQuery
                .OrderBy(x => x.Date)
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
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
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await repository.GetDbSetBy<Ticker>().FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var errors = Array.Empty<string>();

            var count = await repository.GetCountAsync(x => x.TickerName == ctxTicker.Name);

            var dateFilteredQuery = repository.QueryFilter(filter.FilterDateExpression<Price>());
            var resultFilteredQuery = repository.QueryFilter(dateFilteredQuery, x => x.TickerName == ctxTicker.Name);
            var paginatedQuery = repository.QueryPaginator(resultFilteredQuery, pagination, x => x.Date);

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var result = await paginatedQuery
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
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
    }
}
