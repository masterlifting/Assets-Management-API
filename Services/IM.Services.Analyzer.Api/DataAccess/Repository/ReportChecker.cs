using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class ReportChecker : IEntityChecker<Report>
    {
        private readonly AnalyzerContext context;
        public ReportChecker(AnalyzerContext context) => this.context = context;

        public async Task<bool> IsAlreadyAsync(Report report)
        {
            if (await context.Reports.FindAsync(report.TickerName, report.Year, report.Quarter ) is null)
                return false;

            Console.WriteLine($" report for '{report.TickerName}' of year: '{report.Year}' and quarter: '{report.Quarter}' is already!");
            return true;
        }
        public bool WithError(Report report) => false;
    }
}
