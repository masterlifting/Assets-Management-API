using CommonServices.RepositoryService;

using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class ReportSourceChecker : IEntityChecker<ReportSource>
    {
        private readonly ReportsContext context;
        public ReportSourceChecker(ReportsContext context) => this.context = context;

        public async Task<bool> IsAlreadyAsync(ReportSource source)
        {
            source.TickerName = source.TickerName.ToUpperInvariant();

            var ctxSources = context.ReportSources.Where(x => x.TickerName == source.TickerName);

            if (await ctxSources.AnyAsync())
            {
                var sourceValues = await ctxSources.Select(x => x.Value.ToUpperInvariant()).ToArrayAsync();

                if (sourceValues.Contains(source.Value.ToUpperInvariant()))
                {
                    Console.WriteLine($"'{source.Value}' is already!");
                    return true;
                }
            }

            return false;
        }
        public bool WithError(ReportSource source)
        {
            if (source is null)
            {
                Console.WriteLine("source is null!");
                return true;
            }

            if (string.IsNullOrWhiteSpace(source.Value))
            {
                Console.WriteLine("source value is out!");
                return true;
            }

            return false;
        }
    }
}
