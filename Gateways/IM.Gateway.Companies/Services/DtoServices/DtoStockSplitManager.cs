
using CommonServices.HttpServices;
using CommonServices.Models.Dto.GatewayCompanies;
using CommonServices.Models.Http;

using IM.Gateway.Companies.DataAccess.Entities;
using IM.Gateway.Companies.DataAccess.Repository;
using IM.Gateway.Companies.Models.Dto;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using CommonServices.Models.Entity;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class DtoStockSplitManager
    {
        private readonly RepositorySet<StockSplit> repository;
        public DtoStockSplitManager(RepositorySet<StockSplit> repository) => this.repository = repository;

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
            var ctxEntity = new StockSplit()
            {
                CompanyTicker = model.Ticker,
                Date = model.Date,
                Divider = model.Divider
            };
            var message = $"stock split for: '{model.TickerName}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, _) = await repository.CreateAsync(ctxEntity, message);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(StockSplitPostDto model)
        {
            var ctxEntity = new StockSplit
            {
                CompanyTicker = model.Ticker,
                Date = model.Date,
                Divider = model.Divider
            };

            var message = $"stock split for: '{model.TickerName}' of date: {model.Date:yyyy MMMM dd}";
            var (errors, _) = await repository.UpdateAsync(ctxEntity, message);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(PriceIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"stock split for: '{ticker}' of date: {identity.Date:yyyy MMMM dd}";
            var errors = await repository.DeleteAsync(message, ticker, identity.Date);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "deleted" };
        }
    }
}
