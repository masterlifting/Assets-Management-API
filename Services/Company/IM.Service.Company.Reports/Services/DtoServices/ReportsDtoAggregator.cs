using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;

namespace IM.Service.Company.Reports.Services.DtoServices
{
    public class ReportsDtoAggregator
    {
        private readonly RepositorySet<Report> repository;
        public ReportsDtoAggregator(RepositorySet<Report> repository) => this.repository = repository;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var reports = repository.QueryFilter(x => x.Year > filter.Year || x.Year == filter.Year && x.Quarter >= filter.Quarter);
            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();

            var queryResult = await reports
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
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
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            var tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await repository.GetDbSetBy<Ticker>().FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var filteredReports = repository.QueryFilter(x => x.TickerName == ctxTicker.Name && (x.Year > filter.Year || x.Year == filter.Year && x.Quarter >= filter.Quarter));
            var count = await filteredReports.CountAsync();

            var tickers = repository.GetDbSetBy<Ticker>();
            var sourceTypes = repository.GetDbSetBy<SourceType>();
            var paginatedReports= repository.QueryPaginatedResult(filteredReports, pagination, x => x.Year, x => x.Quarter);

            var result = await paginatedReports
                .Join(tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(sourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
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
