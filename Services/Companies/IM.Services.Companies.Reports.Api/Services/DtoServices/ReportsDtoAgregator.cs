using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Services.Companies.Reports.Api.DataAccess;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;

namespace IM.Services.Companies.Reports.Api.Services.DtoServices
{
    public class ReportsDtoAgregator
    {
        private readonly ReportsContext context;
        public ReportsDtoAgregator(ReportsContext context) => this.context = context;

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var query = context.Reports.Join(context.ReportSources, x => x.ReportSourceId, y => y.Id, (x, _) => x).AsQueryable();
            int count = await query.CountAsync();

            var reports = await query
               .OrderByDescending(x => x.Year)
               .ThenByDescending(x => x.Quarter)
               .Skip((pagination.Page - 1) * pagination.Limit)
               .Take(pagination.Limit)
               .Join(context.ReportSources, x => x.ReportSourceId, y => y.Id, (x, y) => new { report = x, sourceTypeId = y.ReportSourceTypeId, ticker = y.TickerName })
               .Join(context.ReportSourceTypes, x => x.sourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.report, y.Name, x.ticker))
               .ToArrayAsync();

            var lastReports = reports.GroupBy(x => x.Ticker).Select(x => x.First()).ToArray();

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

            var reportSources = context.ReportSources.Where(x => x.TickerName.Equals(tickerName));
            int count = await reportSources.Join(context.Reports, x => x.Id, y => y.ReportSourceId, (_, _) => 1).SumAsync();

            var tickerReports = await context.Reports
                .Join(reportSources, x => x.ReportSourceId, y => y.Id, (x, _) => x)
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Join(reportSources, x => x.ReportSourceId, y => y.Id, (x, y) => new { report = x, sourceTypeId = y.ReportSourceTypeId })
                .Join(context.ReportSourceTypes, x => x.sourceTypeId, y => y.Id, (x, y) => new Models.Dto.ReportDto(x.report, y.Name, tickerName))
                .ToArrayAsync();

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
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, int sourceId, FilterRequestModel filter, PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();
            string tickerName = ticker.ToUpperInvariant();
            var _ticker = await context.Tickers.FindAsync(tickerName);

            if (_ticker is null)
                return new()
                {
                    Errors = new string[] { "Ticker not found" }
                };

            var reportSource = _ticker.ReportSources.FirstOrDefault(x => x.Id == sourceId);

            if (reportSource is null)
                return new()
                {
                    Errors = new string[] { "Report source not found" }
                };

            var sourceType = Enum.Parse<ReportSourceTypes>(reportSource.ReportSourceTypeId.ToString()).ToString();
            var count = await context.Reports.Where(x => x.ReportSourceId == reportSource.Id).CountAsync();

            var reports = await context.Reports
                .Where(x => x.ReportSourceId == reportSource.Id && filter.FilterQuarter(x.Year, x.Quarter))
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Quarter)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(x => new Models.Dto.ReportDto(x, sourceType, tickerName))
                .ToArrayAsync();

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
