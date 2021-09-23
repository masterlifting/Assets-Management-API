using CommonServices;
using CommonServices.HttpServices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.Models.Entity;
using CommonServices.Models.Http;
using CommonServices.RabbitServices;

using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;
using IM.Service.Company.Reports.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace IM.Service.Company.Reports.Services.DtoServices
{
    public class DtoManager
    {
        private readonly RepositorySet<Report> repository;
        private readonly string rabbitConnectionString;
        public DtoManager(RepositorySet<Report> repository, IOptions<ServiceSettings> options)
        {
            this.repository = repository;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<ResponseModel<ReportGetDto>> GetAsync(ReportIdentity identity)
        {
            var tickerName = identity.TickerName.ToUpperInvariant().Trim();
            var result = await repository.FindAsync(tickerName, identity.Year, identity.Quarter);

            if (result is null)
                return new() { Errors = new[] { "report not found" } };

            return new()
            {
                Errors = Array.Empty<string>(),
                Data = new()
                {
                    TickerName = result.TickerName,
                    Year = result.Year,
                    Quarter = result.Quarter,
                    SourceType = result.SourceType,
                    Multiplier = result.Multiplier,
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

            var result = await paginatedQuery.Select(x => new ReportGetDto
            {
                TickerName = x.TickerName,
                Year = x.Year,
                Quarter = x.Quarter,
                SourceType = x.SourceType,
                Multiplier = x.Multiplier,
                StockVolume = x.StockVolume,
                Asset = x.Asset,
                CashFlow = x.CashFlow,
                Dividend = x.Dividend,
                LongTermDebt = x.LongTermDebt,
                Obligation = x.Obligation,
                ProfitGross = x.ProfitGross,
                ProfitNet = x.ProfitNet,
                Revenue = x.Revenue,
                ShareCapital = x.ShareCapital,
                Turnover = x.Turnover
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

            var queryResult = await filteredQuery
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .Select(x => new ReportGetDto
                {
                    TickerName = x.TickerName,
                    Year = x.Year,
                    Quarter = x.Quarter,
                    SourceType = x.SourceType,
                    Multiplier = x.Multiplier,
                    StockVolume = x.StockVolume,
                    Asset = x.Asset,
                    CashFlow = x.CashFlow,
                    Dividend = x.Dividend,
                    LongTermDebt = x.LongTermDebt,
                    Obligation = x.Obligation,
                    ProfitGross = x.ProfitGross,
                    ProfitNet = x.ProfitNet,
                    Revenue = x.Revenue,
                    ShareCapital = x.ShareCapital,
                    Turnover = x.Turnover
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
                SourceType = model.SourceType,
                Multiplier = model.Multiplier,
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
            var (errors, report) = await repository.CreateAsync(ctxEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new ReportGetDto
                {
                    TickerName = report!.TickerName,
                    Year = report.Year,
                    Quarter = report.Quarter,
                    SourceType = report.SourceType
                }));

            return new() { Data = message + " created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(ReportPostDto model)
        {
            var ctxEntity = new Report
            {
                TickerName = model.TickerName,
                Year = model.Year,
                Quarter = model.Quarter,
                SourceType = model.SourceType,
                Multiplier = model.Multiplier,
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

            var (errors, report) = await repository.UpdateAsync(ctxEntity, message);

            if (errors.Any())
                return new() { Errors = errors };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new ReportGetDto
                {
                    TickerName = report!.TickerName,
                    Year = report.Year,
                    Quarter = report.Quarter,
                    SourceType = report.SourceType
                }));

            return new() { Data = message + " updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(ReportIdentity identity)
        {
            var ticker = identity.TickerName.ToUpperInvariant().Trim();
            var message = $"report for: '{ticker}' of year: {identity.Year} quarter: {identity.Quarter}";

            var errors = await repository.DeleteAsync(message, ticker, identity.Year, identity.Quarter);

            if (errors.Any())
                return new() { Errors = errors };

            //set to queue
            var previousReports = await repository.GetAnyAsync(x => x.TickerName == identity.TickerName);

            if (!previousReports)
                return new() { Data = message + " deleted. But not set in analyzer!" };

            Report? previousReport = null;
            var previousYear = identity.Year;
            var previousQuarter = identity.Quarter;

            while (previousReport is null)
            {
                (previousYear, previousQuarter) = CommonHelper.SubtractQuarter(previousYear, previousQuarter);
                previousReport = await repository.FindAsync(identity.TickerName, previousYear, previousQuarter);
            }

            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.SetLogic
                , JsonSerializer.Serialize(new ReportGetDto
                {
                    TickerName = previousReport!.TickerName,
                    Year = previousReport.Year,
                    Quarter = previousReport.Quarter,
                    SourceType = previousReport.SourceType
                }));

            return new() { Data = message + " deleted" };
        }
    }
}
