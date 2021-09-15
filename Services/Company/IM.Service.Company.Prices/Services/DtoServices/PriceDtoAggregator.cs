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

        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var prices = repository.QueryFindResult(x => x.Date.Year > filter.Year || x.Date.Year == filter.Year && (x.Date.Month == filter.Month && x.Date.Day >= filter.Day || x.Date.Month > filter.Month));
            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var queryResult = await prices
                .OrderBy(x => x.Date)
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) =>
                    new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
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
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            var tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await repository.GetDbSetBy<Ticker>().FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var filteredPrices = repository.QueryFindResult(x => x.TickerName == ctxTicker.Name && (x.Date.Year > filter.Year || x.Date.Year == filter.Year && (x.Date.Month == filter.Month && x.Date.Day >= filter.Day || x.Date.Month > filter.Month)));
            var count = await filteredPrices.CountAsync();

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();
            var paginatedReports = repository.QueryPaginatedResult(filteredPrices, pagination, x => x.Date);

            var result = await paginatedReports
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Price = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) =>
                    new Models.Dto.PriceDto(x.Price, x.SourceTypeId, y.Name))
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
