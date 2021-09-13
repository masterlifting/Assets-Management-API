using CommonServices;
using CommonServices.Models.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;
using CommonServices.Models.Dto.Http;
using IM.Services.Company.Analyzer.Clients;
using IM.Services.Company.Analyzer.DataAccess.Entities;
using IM.Services.Company.Analyzer.DataAccess.Repository;
using IM.Services.Company.Analyzer.Services.CalculatorServices.Interfaces;
using static IM.Services.Company.Analyzer.Enums;

namespace IM.Services.Company.Analyzer.Services.CalculatorServices
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
            foreach (var t in prices)
                t.StatusId = (byte)Enums.StatusType.Calculating;

            var (errors, _) = await repository.UpdateAsync(prices, $"prices set calculating status count: {prices.Length}");

            return !errors.Any();
        }
        public async Task<bool> CalculateAsync()
        {
            var prices = await repository.FindAsync(x =>
                    x.StatusId == (byte)Enums.StatusType.ToCalculate
                    || x.StatusId == (byte)Enums.StatusType.CalculatedPartial
                    || x.StatusId == (byte)Enums.StatusType.Error);

            if (!prices.Any())
                return false;

            if (await IsSetCalculatingStatusAsync(prices))
                foreach (var priceGroup in prices.GroupBy(x => x.TickerName))
                {
                    var firstElement = priceGroup.OrderBy(x => x.Date).First();
                    Price[] result = priceGroup.ToArray();

                    var targetDate = CommonHelper.GetExchangeLastWorkday(firstElement.SourceType, firstElement.Date);

                    try
                    {
                        var response = await pricesClient.GetPricesAsync(priceGroup.Key, new FilterRequestModel(targetDate.Year, targetDate.Month, targetDate.Day), new PaginationRequestModel(1, int.MaxValue));

                        if (response.Errors.Any() || response.Data!.Count <= 0)
                            continue;

                        var calculator = new PriceComporator(response.Data.Items);
                        result = calculator.GetComparedSample();
                    }
                    catch (Exception ex)
                    {
                        foreach (var price in result)
                            price.StatusId = (byte)Enums.StatusType.Error;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"calculating prices for {priceGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }

                    await repository.CreateUpdateAsync(result, new PriceComparer(), "analyzer prices");
                }

            return true;
        }
    }
}
