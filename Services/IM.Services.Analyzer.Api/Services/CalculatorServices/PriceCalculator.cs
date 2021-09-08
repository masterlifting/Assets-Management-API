using CommonServices;
using CommonServices.Models.Entity;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.DataAccess.Repository;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using System;
using System.Linq;
using System.Threading.Tasks;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class PriceCalculator : IAnalyzerCalculator<Price>
    {
        private readonly RepositorySet<Price> repository;
        private readonly PricesClient pricesClient;

        public PriceCalculator(RepositorySet<Price> repository, PricesClient pricesClient)
        {
            this.repository = repository;
            this.pricesClient = pricesClient;
        }

        public async Task<bool> IsSetCalculatingStatusAsync(Price[] prices)
        {
            for (int i = 0; i < prices.Length; i++)
                prices[i].StatusId = (byte)StatusType.Calculating;
            
            var (errors, _) = await repository.UpdateAsync(prices, $"prices set calculating status count: {prices.Length}");

            return !errors.Any();
        }
        public async Task CalculateAsync()
        {
            var prices = await repository.FindAsync(x =>
                    x.StatusId == ((byte)StatusType.ToCalculate)
                    || x.StatusId == ((byte)StatusType.CalculatedPartial)
                    || x.StatusId == ((byte)StatusType.Error));

            if (prices.Any() && await IsSetCalculatingStatusAsync(prices))
                foreach (var priceGroup in prices.GroupBy(x => x.TickerName))
                {
                    var firstElement = priceGroup.OrderBy(x => x.Date).First();

                    if (firstElement.SourceType is null)
                        continue;

                    var targetDate = CommonHelper.GetExchangeLastWorkday(firstElement.SourceType, firstElement.Date);

                    var response = await pricesClient.GetPricesAsync(priceGroup.Key, new(targetDate.Year, targetDate.Month, targetDate.Day), new(1, int.MaxValue));

                    if (!response.Errors.Any() && response.Data!.Count > 0)
                    {
                        Price[] result = priceGroup.ToArray();

                        try
                        {
                            var calculator = new PriceComporator(response.Data.Items);
                            result = calculator.GetComparedSample();
                        }
                        catch (Exception ex)
                        {
                            for (int j = 0; j < result.Length; j++)
                                result[j].StatusId = (byte)StatusType.Error;

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"calculating prices for {priceGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        await repository.CreateUpdateAsync(result, new PriceComparer(), "analyzer prices");
                    }
                }
        }
    }
}
