using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.DataAccess.Entities;

using System;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.DataAccess.Repository
{
    public class PriceChecker : IEntityChecker<Price>
    {
        private readonly AnalyzerContext context;
        public PriceChecker(AnalyzerContext context) => this.context = context;

        public async Task<bool> IsAlreadyAsync(Price price)
        {
            if (await context.Prices.FindAsync(price.TickerName, price.Date ) is null)
                return false;

            Console.WriteLine($"price for '{price.TickerName}' of date: '{price.Date:dd.MM.yyyy}' is already!");
            return true;
        }
        public bool WithError(Price price) => false;
    }
}
