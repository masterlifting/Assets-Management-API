using IM.Services.Companies.Reports.Api.DataAccess;
using IM.Services.Companies.Reports.Api.Models;
using IM.Services.Companies.Reports.Api.Models.Dto;
using IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.Agregators.Implementations
{
    public class LastReportDtoAgregator : ILastReportDtoAgregator
    {
        private readonly ReportsContext context;
        public LastReportDtoAgregator(ReportsContext context) => this.context = context;

        public async Task<ResponseModel<ReportDto>> GetReportAsync(string ticker)
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
            var report = await context.Reports
               .Join(reportSources, x => x.ReportSourceId, y => y.Id, (x, _) => x)
               .OrderBy(x => x.Year)
               .ThenBy(x => x.Quarter)
               .LastOrDefaultAsync();

            var source = await context.ReportSources.FindAsync(report.ReportSourceId);

            return new()
            {
                Errors = errors,
                Data = new(report, source.ReportSourceType.Name, tickerName)
            };
        }
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
               .Join(context.ReportSourceTypes, x => x.sourceTypeId, y => y.Id, (x, y) => new ReportDto(x.report, y.Name, x.ticker))
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