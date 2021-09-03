using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Companies.Reports.Api.DataAccess;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.DtoServices
{
    public class ReportsDtoAgregator
    {
        private readonly ReportsContext context;
        public ReportsDtoAgregator(ReportsContext context) => this.context = context;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            int count = await context.Reports.CountAsync();

            var reports = await context.Reports
               .OrderByDescending(x => x.Year)
               .ThenByDescending(x => x.Quarter)
               .Skip((pagination.Page - 1) * pagination.Limit)
               .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
               .ToArrayAsync();

            var lastReports = reports.GroupBy(x => x.TickerName).Select(x => x.First()).ToArray();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = lastReports,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            var count = _ticker.Reports.Count();

            var tickerReports = _ticker.Reports
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
                .ToArray();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = tickerReports,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            var count = _ticker.Reports.Count();

            var reports = _ticker.Reports
                .Where(x => filter.FilterQuarter(x.Year, x.Quarter))
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(context.Tickers, x => x.TickerName, y => y.Name, (x, y) => new { Report = x, y.SourceTypeId, })
                .Join(context.SourceTypes, x => x.SourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.Report, x.SourceTypeId, y.Name))
                .ToArray();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = reports,
                    Count = count
                }
            };
        }
    }
}
