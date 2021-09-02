using CommonServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class PriceCalculator : IAnalyzerCalculator
    {
        private readonly IServiceProvider services;
        private readonly AnalyzerContext context;
        private readonly PricesClient pricesClient;

        public PriceCalculator(IServiceProvider services, AnalyzerContext context, PricesClient pricesClient)
        {
            this.services = services;
            this.context = context;
            this.pricesClient = pricesClient;
        }
        async Task<Price[]> GetPricesAsync() => await context.Prices.Where(x =>
                     x.StatusId == ((byte)StatusType.ToCalculate)
                     && x.StatusId == ((byte)StatusType.CalculatedPartial)
                     && x.StatusId == ((byte)StatusType.Error))
                    .ToArrayAsync();
        async Task<bool> IsSetCalculatingStatusAsync(Price[] prices)
        {
            for (int i = 0; i < prices.Length; i++)
                prices[i].StatusId = (byte)StatusType.Calculating;

            return await context.SaveChangesAsync() == prices.Length;
        }
        public async Task CalculateAsync()
        {
            var prices = await GetPricesAsync();

            if (prices.Any() && await IsSetCalculatingStatusAsync(prices))
            {
                var repository = services.CreateScope().ServiceProvider.GetRequiredService<EntityRepository<Price, AnalyzerContext>>();

                var tickerPrices = prices.GroupBy(x => x.TickerName);
                foreach (var i in tickerPrices)
                {
                    var targetPrices = i.OrderBy(x => x.Date);
                    var firstPrice = targetPrices.First();
                    var priceTargetDate = CommonHelper.GetExchangeLastWorkday(Enum.Parse<PriceSourceTypes>(firstPrice.PriceSourceTypeId.ToString()));

                    var pricesResponse = await pricesClient.GetPricesAsync(i.Key, new(priceTargetDate.Year, priceTargetDate.Month, priceTargetDate.Day), new(1, int.MaxValue));

                    if (!pricesResponse.Errors.Any() && pricesResponse.Data!.Count > 0)
                    {
                        Price[] result = targetPrices.ToArray();

                        try
                        {
                            var calculator = new PriceComporator(pricesResponse.Data.Items);
                            result = calculator.GetCoparedSample();
                        }
                        catch (Exception ex)
                        {
                            for (int j = 0; j < result.Length; j++)
                                result[j].StatusId = (byte)StatusType.Error;

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Calculating prices for {i.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        for (int j = 0; j < result.Length; j++)
                            await repository.CreateOrUpdateAsync(new { result[j].TickerName, result[j].Date }, result[j], $"analyzer price for {result[j].TickerName}");
                    }
                }
            }
        }
    }
}
