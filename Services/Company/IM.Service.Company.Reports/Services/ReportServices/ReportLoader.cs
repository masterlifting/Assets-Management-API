using CommonServices.Models.Entity;

using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;
using IM.Service.Company.Reports.Models;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommonServices;
using CommonServices.Models.Dto.CompanyReports;
using CommonServices.RabbitServices;
using IM.Service.Company.Reports.Settings;
using Microsoft.Extensions.Options;

namespace IM.Service.Company.Reports.Services.ReportServices
{
    public class ReportLoader
    {
        private readonly RepositorySet<Report> repository;
        private readonly ReportParser parser;
        private readonly string rabbitConnectionString;
        public ReportLoader(IOptions<ServiceSettings> options, RepositorySet<Report> repository, ReportParser parser)
        {
            this.repository = repository;
            this.parser = parser;
            rabbitConnectionString = options.Value.ConnectionStrings.Mq;
        }

        public async Task<Report[]> LoadAsync(Ticker ticker)
        {
            var result = Array.Empty<Report>();
            var ctxTicker = await repository.GetDbSetBy<Ticker>().FindAsync(ticker.Name);

            if (ctxTicker is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Ticker '{ticker.Name}' not found");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            if (ctxTicker.SourceValue is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Source value for '{ticker.Name}' not found");
                Console.ForegroundColor = ConsoleColor.Gray;
                return result;
            }

            var lastReport = await GetLastReportAsync(ctxTicker);
            var source = await repository.GetDbSetBy<SourceType>().FindAsync(ctxTicker.SourceTypeId);

            var config = lastReport is null
                ? new ReportLoaderData(ctxTicker.SourceValue)
                {
                    TickerName = ctxTicker.Name,
                    Year = DateTime.UtcNow.AddYears(-3).Year,
                    Quarter = 1
                }
                : new ReportLoaderData(ctxTicker.SourceValue)
                {
                    TickerName = ctxTicker.Name,
                    Year = lastReport.Year,
                    Quarter = lastReport.Quarter
                };

            var reports = await GetReportsAsync(source.Name, config);

            result = await SaveReportsAsync(reports);

            if (result.Length <= 0)
                return result;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Saved reports for {ticker.Name}");
            Console.ForegroundColor = ConsoleColor.Gray;
            return result;
        }
        public async Task<Report[]> LoadAsync()
        {
            var lastReports = await GetLastReportsAsync();
            var lastReportsDic = lastReports.ToDictionary(x => x.TickerName, y => (y.Year, y.Quarter));

            var tickers = await repository.GetDbSetBy<Ticker>()
                .Where(x => x.SourceValue != null)
                .Select(x => new { x.Name, x.SourceValue, x.SourceTypeId })
                .ToArrayAsync();

            var sources = await repository.GetDbSetBy<SourceType>()
                .Select(x => new { x.Id, x.Name })
                .ToArrayAsync();

            var result = new List<Report>(tickers.Length * 5);

            foreach (var item in tickers
                .GroupBy(x => x.SourceTypeId)
                .Join(sources, x => x.Key, y => y.Id, (x, y) =>
                    new { Source = y.Name, Tickers = x }))
            {
                var data = item.Tickers
                    .Select(x => lastReportsDic.ContainsKey(x.Name)
                        ? new ReportLoaderData(x.SourceValue!)
                        {
                            TickerName = x.Name,
                            Year = lastReportsDic[x.Name].Year,
                            Quarter = lastReportsDic[x.Name].Quarter
                        }
                        : new ReportLoaderData(x.SourceValue!)
                        {
                            TickerName = x.Name,
                            Year = DateTime.UtcNow.AddYears(-3).Year,
                            Quarter = 1
                        })
                    .ToArray();

                if (!data.Any())
                    continue;

                var reports = await GetReportsAsync(item.Source, data);
                result.AddRange(await SaveReportsAsync(reports));
            }

            if (result.Count <= 0)
                return Array.Empty<Report>();

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Saved data for '{result.Count}' tickers");
            Console.ForegroundColor = ConsoleColor.Gray;
            return result.ToArray();
        }

        private async Task<Report?> GetLastReportAsync(TickerIdentity ticker)
        {
            var reports = await repository.GetSampleAsync(x => x.TickerName == ticker.Name && x.Year >= DateTime.UtcNow.AddYears(-3).Year);
            return reports
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Quarter)
                .LastOrDefault();
        }
        private async Task<Report[]> GetLastReportsAsync()
        {
            var reports = await repository.GetSampleAsync(x => x.Year >= DateTime.UtcNow.AddYears(-3).Year);
            return reports
                .GroupBy(x => x.TickerName)
                .Select(x =>
                    x.OrderBy(y => y.Year)
                    .ThenBy(y => y.Quarter)
                    .Last())
                .ToArray();
        }

        private async Task<Report[]> GetReportsAsync(string source, ReportLoaderData data)
        {
            return IsMissingLastQuarter(data.Year, data.Quarter)
                ? await parser.GetReportsAsync(source, data)
                : Array.Empty<Report>();
        }
        private async Task<Report[]> GetReportsAsync(string source, ReportLoaderData[] data)
        {
            return await parser.GetReportsAsync(source, data.Where(x => IsMissingLastQuarter(x.Year, x.Quarter)));
        }

        private async Task<Report[]> SaveReportsAsync(IEnumerable<Report> reports)
        {
            var (errors, result) = await repository.CreateUpdateAsync(reports, new ReportComparer(), "reports");

            if (errors.Any()) 
                return result;
            
            var publisher = new RabbitPublisher(rabbitConnectionString, QueueExchanges.Logic);

            foreach (var report in result)
                publisher.PublishTask(
                    QueueNames.CompanyAnalyzer
                    , QueueEntities.Report
                    , QueueActions.SetLogic
                    , JsonSerializer.Serialize(new ReportGetDto
                    {
                        TickerName = report.TickerName,
                        SourceType = report.SourceType,
                        Year = report.Year,
                        Quarter = report.Quarter
                    }));

            return result;
        }

        private static bool IsMissingLastQuarter(int lastYear, byte lastQuarter)
        {
            var (controlYear, controlQuarter) = CommonHelper.SubtractQuarter(DateTime.UtcNow);

            var isNew = controlYear > lastYear || controlYear == lastYear && controlQuarter > lastQuarter;

            if (isNew)
                return isNew;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Last quarter is actual. Reports will not be loaded.");
            Console.ForegroundColor = ConsoleColor.Gray;

            return isNew;
        }
    }
}