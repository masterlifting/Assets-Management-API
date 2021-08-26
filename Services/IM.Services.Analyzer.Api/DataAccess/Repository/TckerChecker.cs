using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class TckerChecker : IEntityChecker<Ticker>
    {
        private readonly AnalyzerContext context;
        public TckerChecker(AnalyzerContext context) => this.context = context;

        public async Task<bool> IsAlreadyAsync(Ticker ticker)
        {
            ticker.Name = ticker.Name.ToUpperInvariant();

            if (await context.Tickers.FindAsync(ticker.Name) is null)
                return false;

            Console.WriteLine($"'{ticker.Name}' is already!");
            return true;
        }
        public bool WithError(Ticker ticker) => false;
    }
}
