using CommonServices;
using CommonServices.RepositoryService;

using IM.Services.Analyzer.Api.Clients;
using IM.Services.Analyzer.Api.DataAccess;
using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class PriceCalculator : IAnalyzerCalculator<Price>
    {
        private readonly AnalyzerContext context;
        private readonly EntityRepository<Price, AnalyzerContext> repository;
        private readonly PricesClient pricesClient;

        public PriceCalculator(AnalyzerContext context, EntityRepository<Price, AnalyzerContext> repository, PricesClient pricesClient)
        {
            this.context = context;
            this.repository = repository;
            this.pricesClient = pricesClient;
        }

        public async Task<Price[]> GetDataAsync() => await context.Prices.Where(x =>
                     x.StatusId == ((byte)StatusType.ToCalculate)
                     || x.StatusId == ((byte)StatusType.CalculatedPartial)
                     || x.StatusId == ((byte)StatusType.Error))
                    .ToArrayAsync();
        public async Task<bool> IsSetCalculatingStatusAsync(Price[] prices)
        {
            for (int i = 0; i < prices.Length; i++)
                prices[i].StatusId = (byte)StatusType.Calculating;

            return await context.SaveChangesAsync() == prices.Length;
        }
        public async Task CalculateAsync()
        {
            var prices = await GetDataAsync();

            if (prices.Any() && await IsSetCalculatingStatusAsync(prices))
                foreach (var priceGroup in prices.GroupBy(x => x.TickerName))
                {
                    var firstPrice = priceGroup.First();
                    var priceTargetDate = CommonHelper.GetExchangeLastWorkday(Enum.Parse<PriceSourceTypes>(firstPrice.SourceTypeId.ToString()), firstPrice.Date);

                    var pricesResponse = await pricesClient.GetPricesAsync(priceGroup.Key, new(priceTargetDate.Year, priceTargetDate.Month, priceTargetDate.Day), new(1, int.MaxValue));

                    if (!pricesResponse.Errors.Any() && pricesResponse.Data!.Count > 0)
                    {
                        Price[] result = priceGroup.ToArray();

                        try
                        {
                            var calculator = new PriceComporator(pricesResponse.Data.Items);
                            result = calculator.GetComparedSample();
                        }
                        catch (Exception ex)
                        {
                            for (int j = 0; j < result.Length; j++)
                                result[j].StatusId = (byte)StatusType.Error;

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Calculating prices for {priceGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }

                        for (int j = 0; j < result.Length; j++)
                            await repository.CreateOrUpdateAsync(result[j], $"analyzer price for {result[j].TickerName}");
                    }
                }
        }
    }
}
