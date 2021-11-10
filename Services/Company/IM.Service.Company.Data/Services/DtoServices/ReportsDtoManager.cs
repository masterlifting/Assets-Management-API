using IM.Service.Common.Net.HttpServices;
using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;
using IM.Service.Common.Net.RabbitServices;

using IM.Service.Company.Data.DataAccess.Entities;
using IM.Service.Company.Data.DataAccess.Repository;
using IM.Service.Company.Data.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using IM.Service.Common.Net.RepositoryService.Comparators;
using IM.Service.Common.Net.RepositoryService.Filters;

namespace IM.Service.Company.Data.Services.DtoServices
{
    public class ReportsDtoManager
    {
        private readonly RepositorySet<Report> reportRepository;
        private readonly RepositorySet<DataAccess.Entities.Company> companyRepository;
        private readonly string rabbitConnectionString;
        public ReportsDtoManager(RepositorySet<Report> reportRepository, RepositorySet<DataAccess.Entities.Company> companyRepository, IOptions<ServiceSettings> options)
        {
            this.reportRepository = reportRepository;
            this.companyRepository = companyRepository;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<ResponseModel<ReportGetDto>> GetAsync(string companyId, int year, byte quarter)
        {
            var company = await companyRepository.FindAsync(companyId.ToUpperInvariant().Trim());

            if (company is null)
                return new() { Errors = new[] { "company not found" } };

            var report = await reportRepository.FindAsync(company.Id, year, quarter);

            if (report is null)
                return new() { Errors = new[] { "report not found" } };

            return new()
            {
                Data = new()
                {
                    Ticker = company.Id,
                    Company = company.Name,
                    Year = report.Year,
                    Quarter = report.Quarter,
                    SourceType = report.SourceType,
                    Multiplier = report.Multiplier,
                    Asset = report.Asset,
                    CashFlow = report.CashFlow,
                    LongTermDebt = report.LongTermDebt,
                    Obligation = report.Obligation,
                    ProfitGross = report.ProfitGross,
                    ProfitNet = report.ProfitNet,
                    Revenue = report.Revenue,
                    ShareCapital = report.ShareCapital,
                    Turnover = report.Turnover
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetAsync(CompanyDataFilterByQuarter<Report> filter, HttpPagination pagination)
        {
            var filteredQuery = reportRepository.GetQuery(filter.FilterExpression);
            var count = await reportRepository.GetCountAsync(filteredQuery);
            var paginatedQuery = reportRepository.GetPaginationQuery(filteredQuery, pagination, x => x.Year, x => x.Quarter);

            var result = await paginatedQuery
                .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new ReportGetDto
                {
                    Ticker = y.Id,
                    Company = y.Name,
                    Year = x.Year,
                    Quarter = x.Quarter,
                    SourceType = x.SourceType,
                    Multiplier = x.Multiplier,
                    Asset = x.Asset,
                    CashFlow = x.CashFlow,
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
                Data = new()
                {
                    Items = result,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<PaginatedModel<ReportGetDto>>> GetLastAsync(CompanyDataFilterByQuarter<Report> filter, HttpPagination pagination)
        {
            var filteredQuery = reportRepository.GetQuery(filter.FilterExpression);

            var queryResult = await filteredQuery
                .Join(companyRepository.GetDbSet(), x => x.CompanyId, y => y.Id, (x, y) => new ReportGetDto
                {
                    Ticker = y.Id,
                    Company = y.Name,
                    Year = x.Year,
                    Quarter = x.Quarter,
                    SourceType = x.SourceType,
                    Multiplier = x.Multiplier,
                    Asset = x.Asset,
                    CashFlow = x.CashFlow,
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
                .GroupBy(x => x.Company)
                .Select(x => x
                    .OrderBy(y => y.Year)
                    .ThenBy(y => y.Quarter)
                    .Last())
                .ToArray();

            return new()
            {
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
                CompanyId = model.CompanyId,
                Year = model.Year,
                Quarter = model.Quarter,
                SourceType = model.SourceType,
                Multiplier = model.Multiplier,
                Asset = model.Asset,
                CashFlow = model.CashFlow,
                LongTermDebt = model.LongTermDebt,
                Obligation = model.Obligation,
                ProfitGross = model.ProfitGross,
                ProfitNet = model.ProfitNet,
                Revenue = model.Revenue,
                ShareCapital = model.ShareCapital,
                Turnover = model.Turnover
            };

            var message = $"report of '{model.CompanyId}' create at {model.Year} - {model.Quarter}";
            var (error, report) = await reportRepository.CreateAsync(ctxEntity, message);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.Call
                , JsonSerializer.Serialize(new ReportIdentityDto
                {
                    CompanyId = report!.CompanyId,
                    Year = report.Year,
                    Quarter = report.Quarter,
                }));

            return new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> CreateAsync(IEnumerable<ReportPostDto> models)
        {
            var reports = models.ToArray();

            if (!reports.Any())
                return new() { Errors = new[] { "report data for creating not found" } };

            var ctxEntities = reports.GroupBy(x => (x.Year, x.Quarter)).Select(x => new Report
            {
                CompanyId = x.Last().CompanyId,
                Year = x.Last().Year,
                Quarter = x.Last().Quarter,
                SourceType = x.Last().SourceType,
                Multiplier = x.Last().Multiplier,
                Asset = x.Last().Asset,
                CashFlow = x.Last().CashFlow,
                LongTermDebt = x.Last().LongTermDebt,
                Obligation = x.Last().Obligation,
                ProfitGross = x.Last().ProfitGross,
                ProfitNet = x.Last().ProfitNet,
                Revenue = x.Last().Revenue,
                ShareCapital = x.Last().ShareCapital,
                Turnover = x.Last().Turnover
            });

            var message = $"reports of '{reports[0].CompanyId}' create";
            var (error, _) = await reportRepository.CreateAsync(ctxEntities, new CompanyQuarterComparer<Report>(), message);

            return error is not null
                ? new() { Errors = new[] { error } }
                : new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(ReportPostDto model)
        {
            var ctxEntity = new Report
            {
                CompanyId = model.CompanyId,
                Year = model.Year,
                Quarter = model.Quarter,
                SourceType = model.SourceType,
                Multiplier = model.Multiplier,
                Asset = model.Asset,
                CashFlow = model.CashFlow,
                LongTermDebt = model.LongTermDebt,
                Obligation = model.Obligation,
                ProfitGross = model.ProfitGross,
                ProfitNet = model.ProfitNet,
                Revenue = model.Revenue,
                ShareCapital = model.ShareCapital,
                Turnover = model.Turnover
            };

            var message = $"report of '{model.CompanyId}' update at {model.Year} - {model.Quarter}";

            var (error, report) = await reportRepository.UpdateAsync(ctxEntity, message);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.Call
                , JsonSerializer.Serialize(new ReportIdentityDto
                {
                    CompanyId = report!.CompanyId,
                    Year = report.Year,
                    Quarter = report.Quarter,
                }));

            return new() { Data = message + " success" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(string companyId, int year, byte quarter)
        {
            companyId = companyId.ToUpperInvariant().Trim();
            var message = $"report of '{companyId}' delete at {year} - {quarter}";

            var (error, deletedEntity) = await reportRepository.DeleteAsync(message, companyId, year, quarter);

            if (error is not null)
                return new() { Errors = new[] { error } };

            //set to queue
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Function);
            publisher.PublishTask(
                QueueNames.CompanyAnalyzer
                , QueueEntities.Report
                , QueueActions.Call
                , JsonSerializer.Serialize(new ReportIdentityDto
                {
                    CompanyId = deletedEntity!.CompanyId,
                    Year = deletedEntity.Year,
                    Quarter = deletedEntity.Quarter,
                }));

            return new() { Data = message + " success" };
        }
    }
}
