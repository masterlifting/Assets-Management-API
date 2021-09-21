using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Http;

using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using CommonServices.Models.Entity;

namespace IM.Service.Company.Reports.Services.DtoServices
{
    public class DtoManager
    {
        private readonly RepositorySet<Report> repository;
        public DtoManager(RepositorySet<Report> repository) => this.repository = repository;

        public async Task<ResponseModel<ReportGetDto>> GetAsync(ReportIdentity identity)
        {
            var tickerName = identity.TickerName.ToUpperInvariant().Trim();
            var result = await repository.FindAsync(tickerName, identity.Year, identity.Quarter);

            if (result is null)
                return new() { Errors = new[] { "report not found" } };

            var ticker = await repository.GetDbSetBy<Ticker>().FindAsync(tickerName);

            var sourceType = await repository.GetDbSetBy<SourceType>().FindAsync(ticker.SourceTypeId);

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    TickerName = result.TickerName,
                    Year = result.Year,
                    Quarter = result.Quarter,
                    SourceType = sourceType.Name,
                    StockVolume = result.StockVolume,
                    Asset = result.Asset,
                    CashFlow = result.CashFlow,
                    Dividend = result.Dividend,
                    LongTermDebt = result.LongTermDebt,
                    Obligation = result.Obligation,
                    ProfitGross = result.ProfitGross,
                    ProfitNet = result.ProfitNet,
                    Revenue = result.Revenue,
                    ShareCapital = result.ShareCapital,
                    Turnover = result.Turnover
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetAsync(HttpFilter filter, HttpPagination pagination)
        {
            var errors = Array.Empty<string>();

            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);
            var count = await repository.GetCountAsync(filteredQuery);
            var paginatedQuery = repository.GetPaginationQuery(filteredQuery, pagination, x => x.Year, x => x.Quarter);

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var result = await paginatedQuery
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new ReportGetDto
                {
                    TickerName = x.Report.TickerName,
                    Year = x.Report.Year,
                    Quarter = x.Report.Quarter,
                    SourceType = y.Name,
                    StockVolume = x.Report.StockVolume,
                    Asset = x.Report.Asset,
                    CashFlow = x.Report.CashFlow,
                    Dividend = x.Report.Dividend,
                    LongTermDebt = x.Report.LongTermDebt,
                    Obligation = x.Report.Obligation,
                    ProfitGross = x.Report.ProfitGross,
                    ProfitNet = x.Report.ProfitNet,
                    Revenue = x.Report.Revenue,
                    ShareCapital = x.Report.ShareCapital,
                    Turnover = x.Report.Turnover
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
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLastAsync(HttpFilter filter, HttpPagination pagination)
        {
            var filteredQuery = repository.GetFilterQuery(filter.FilterExpression);
            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var queryResult = await filteredQuery
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new ReportGetDto()
                {
                    TickerName = x.Report.TickerName,
                    Year = x.Report.Year,
                    Quarter = x.Report.Quarter,
                    SourceType = y.Name,
                    StockVolume = x.Report.StockVolume,
                    Asset = x.Report.Asset,
                    CashFlow = x.Report.CashFlow,
                    Dividend = x.Report.Dividend,
                    LongTermDebt = x.Report.LongTermDebt,
                    Obligation = x.Report.Obligation,
                    ProfitGross = x.Report.ProfitGross,
                    ProfitNet = x.Report.ProfitNet,
                    Revenue = x.Report.Revenue,
                    ShareCapital = x.Report.ShareCapital,
                    Turnover = x.Report.Turnover
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
        public async Task<ResponseModel<string>> CreateAsync(ReportPostDto model)
        {
            var ctxEntity = new Report
            {
                TickerName = model.TickerName,
                Year = model.Year,
                Quarter = model.Quarter,
                StockVolume = model.StockVolume,
                Asset = model.Asset,
                CashFlow = model.CashFlow,
                Dividend = model.Dividend,
                LongTermDebt = model.LongTermDebt,
                Obligation = model.Obligation,
                ProfitGross = model.ProfitGross,
                ProfitNet = model.ProfitNet,
                Revenue = model.Revenue,
                ShareCapital = model.ShareCapital,
                Turnover = model.Turnover
            };

            var message = $"report for: '{model.TickerName}' of year: {model.Year} quarter: {model.Quarter}";
            var (errors, _) = await repository.CreateAsync(ctxEntity, message);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(ReportPostDto model)
        {
            var ctxEntity = new Report
            {
                TickerName = model.TickerName,
                Year = model.Year,
                Quarter = model.Quarter,
                StockVolume = model.StockVolume,
                Asset = model.Asset,
                CashFlow = model.CashFlow,
                Dividend = model.Dividend,
                LongTermDebt = model.LongTermDebt,
                Obligation = model.Obligation,
                ProfitGross = model.ProfitGross,
                ProfitNet = model.ProfitNet,
                Revenue = model.Revenue,
                ShareCapital = model.ShareCapital,
                Turnover = model.Turnover
            };

            var message = $"report for: '{model.TickerName}' of year: {model.Year} quarter: {model.Quarter}";

            var (errors, _) = await repository.UpdateAsync(ctxEntity, message);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(ReportIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"report for: '{ticker}' of year: {identity.Year} quarter: {identity.Quarter}";

            var errors = await repository.DeleteAsync(message, ticker, identity.Year, identity.Quarter);

            return errors.Any()
                ? new ResponseModel<string> { Errors = errors }
                : new ResponseModel<string> { Data = message + "deleted" };
        }
    }
}
