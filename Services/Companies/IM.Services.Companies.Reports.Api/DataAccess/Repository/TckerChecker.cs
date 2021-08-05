using IM.Services.Companies.Reports.Api.DataAccess.Entities;

using System;
using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.DataAccess.Repository
{
    public class TckerChecker : IEntityChecker<Ticker>
    {
        private readonly ReportsContext context;
        public TckerChecker(ReportsContext context) => this.context = context;

        public async Task<bool> IsAlreadyAsync(Ticker ticker)
        {
            ticker.Name = ticker.Name.ToUpperInvariant();

            if (await context.Tickers.FindAsync(ticker.Name) is null)
                return false;

            Console.WriteLine($"'{ticker.Name}' is already!");
            return true;
        }
        public bool WithError(Ticker ticker)
        {
            if (ticker is null)
            {
                Console.WriteLine("ticker is null");
                return true;
            }

            return false;
        }
    }
}
