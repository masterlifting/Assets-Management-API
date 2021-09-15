using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonServices.Models.Entity;
using IM.Service.Company.Reports.DataAccess;
using IM.Service.Company.Reports.DataAccess.Entities;
using IM.Service.Company.Reports.DataAccess.Repository;
using static IM.Service.Company.Reports.Services.ReportServices.ReportLoaderHelper;

namespace IM.Service.Company.Reports.Services.ReportServices
{
    public class ReportLoader
    {
        private readonly DatabaseContext context;
        private readonly RepositorySet<Report> repository;
        private readonly ReportParser parser;
        public ReportLoader(DatabaseContext context, RepositorySet<Report> repository, ReportParser parser)
        {
            this.context = context;
            this.repository = repository;
            this.parser = parser;
        }

        public async Task<Report[]> LoadReportsAsync(Ticker ticker)
        {
            var result = Array.Empty<Report>();

            var ctxTicker = await context.Tickers.FindAsync(ticker.Name);

            if (ctxTicker is not null)
            {
                var lastReport = await GetLastReportAsync(ctxTicker);

                if (IsMissingLastQuarter(lastReport.Year, lastReport.Quarter))
                {
                    var loadedReports = await parser.GetReportsAsync(ctxTicker);
                    var (toAdd, toUpdate) = SeparateReports(loadedReports, lastReport);
                    result = await SaveReportsAsync(toUpdate, toAdd);
                }
            }

            Console.WriteLine($"\nSaved report count: {result.Length} for {ticker.Name}");

            return result;
        }
        public async Task<Report[]> LoadReportsAsync()
        {
            var tickers = await context.Tickers.ToArrayAsync();
            var result = new List<Report>(tickers.Length);

            foreach (var ticker in tickers)
            {
                var lastReport = await GetLastReportAsync(ticker);

                // ReSharper disable once InvertIf
                if (IsMissingLastQuarter(lastReport.Year, lastReport.Quarter))
                {
                    var loadedReports = await parser.GetReportsAsync(ticker);
                    var (toAdd, toUpdate) = SeparateReports(loadedReports, lastReport);
                    var reports = await SaveReportsAsync(toUpdate, toAdd);
                    result.AddRange(reports);
                }
            }

            return result.ToArray();
        }

        private async Task<Report> GetLastReportAsync(TickerIdentity ticker)
        {
            var reports = await context.Reports.Where(x => x.TickerName == ticker.Name).ToArrayAsync();
            return reports.Any() ? FindLastReport(reports) : new() { TickerName = ticker.Name, Year = 0, Quarter = 0 };
        }
        private async Task<Report[]> SaveReportsAsync(IReadOnlyCollection<Report> toUpdate, IReadOnlyCollection<Report> toAdd)
        {
            var result = new Report[toUpdate.Count + toAdd.Count];

            if (toAdd.Any())
            {
                var (errors, reports) = await repository.CreateAsync(toAdd, new ReportComparer(), "loaded reports");

                if (!errors.Any())
                    result = reports;
            }

            if (toUpdate.Any())
            {
                var (errors, reports) = await repository.UpdateAsync(toUpdate, "updated reports");
                if (!errors.Any())
                    result = result.Concat(reports).ToArray();
            }

            return result;
        }
    }
}