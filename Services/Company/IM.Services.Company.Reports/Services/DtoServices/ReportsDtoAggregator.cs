using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Services.Company.Reports.DataAccess;

namespace IM.Services.Company.Reports.Services.DtoServices
{
    public class ReportsDtoAggregator
    {
        private readonly ReportsContext context;
        public ReportsDtoAggregator(ReportsContext context) => this.context = context;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var reports = context.Reports.Where(x => x.Year > filter.Year || x.Year == filter.Year && x.Quarter >= filter.Quarter);
            
            var queryResult = await reports
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
                .ToArrayAsync();

            var groupedResult = queryResult
                .GroupBy(x => x.TickerName)
                .Select(x => x.Last())
                .ToArray();

            var result = groupedResult
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .ToArray();

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    Items = result,
                    Count = groupedResult.Length
                }
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            var tickerName = ticker.ToUpperInvariant();
            var ctxTicker = await context.Tickers.FindAsync(tickerName);

            if (ctxTicker is null)
                return new()
                {
                    Errors = new[] { "Ticker not found" }
                };

            var reports = context.Reports.Where(x => x.TickerName == ctxTicker.Name && (x.Year > filter.Year || x.Year == filter.Year && x.Quarter >= filter.Quarter));

            var count = await reports.CountAsync();

            var result = await reports
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
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
