using CommonServices;
using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using CommonServices.Models.Entity;

using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class PriceCalculator : IAnalyzerCalculator<Price>
    {
        private readonly RepositorySet<Price> repository;
        private readonly CompanyPricesClient pricesClient;
        private readonly GatewayCompaniesClient gatewayClient;

        public PriceCalculator(
            RepositorySet<Price> repository,
            CompanyPricesClient pricesClient,
            GatewayCompaniesClient gatewayClient)
        {
            this.repository = repository;
            this.pricesClient = pricesClient;
            this.gatewayClient = gatewayClient;
        }

        public async Task<bool> IsSetCalculatingStatusAsync(Price[] prices)
        {
            foreach (var price in prices)
                price.StatusId = (byte)StatusType.Calculating;

            var (errors, _) = await repository.UpdateAsync(prices, $"prices calculating status count: {prices.Length}");

            return !errors.Any();
        }
        public async Task<bool> CalculateAsync()
        {
            var prices = await repository.FindAsync(x =>
                    x.StatusId == (byte)StatusType.ToCalculate
                    || x.StatusId == (byte)StatusType.CalculatedPartial
                    || x.StatusId == (byte)StatusType.Error);

            if (!prices.Any())
                return false;

            foreach (var priceGroup in prices.GroupBy(x => x.TickerName))
            {
                Price[] result = priceGroup.ToArray();

                if (!await IsSetCalculatingStatusAsync(result))
                    continue;

                var firstElement = priceGroup.OrderBy(x => x.Date).First();
                var targetDate = CommonHelper.GetExchangeLastWorkday(firstElement.SourceType, firstElement.Date);

                try
                {
                    var calculatingData = await GetPricesAsync(priceGroup.Key, targetDate);

                    var calculator = new PriceComparator(calculatingData);
                    result = calculator.GetComparedSample();
                }
                catch (Exception ex)
                {
                    foreach (var price in result)
                        price.StatusId = (byte)StatusType.Error;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"calculating prices for {priceGroup.Key} failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                await repository.CreateUpdateAsync(result, new PriceComparer(), "price calculator");
            }

            return true;
        }
        public async Task<bool> CalculateAsync(DateTime dateStart)
        {
            var prices = await repository.FindAsync(x => x.StatusId != (byte)StatusType.Calculating);

            if (!prices.Any())
                return false;

            foreach (var priceGroup in prices.GroupBy(x => x.TickerName))
            {
                Price[] result = priceGroup.ToArray();

                if (!await IsSetCalculatingStatusAsync(result))
                    continue;

                try
                {
                    var calculatingData = await GetPricesAsync(priceGroup.Key, dateStart);

                    var calculator = new PriceComparator(calculatingData);
                    result = calculator.GetComparedSample();
                }
                catch (Exception ex)
                {
                    foreach (var price in result)
                        price.StatusId = (byte)StatusType.Error;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"calculating prices for '{priceGroup.Key}' failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                await repository.CreateUpdateAsync(result, new PriceComparer(), "price calculator");
            }

            return true;
        }
        
        private async Task<IReadOnlyCollection<PriceDto>> GetPricesAsync(string ticker, DateTime date)
        {
            ResponseModel<PaginationResponseModel<StockSplitDto>>? stockSplitsResponse = null;
            ResponseModel<PaginationResponseModel<PriceDto>>? pricesResponse = null;

            try
            {
                stockSplitsResponse = await gatewayClient.GetAsync(ticker, new(date.Year, date.Month, date.Day), new(1, int.MaxValue));
                pricesResponse = await pricesClient.GetAsync(ticker, new(date.Year, date.Month, date.Day), new(1, int.MaxValue));
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"requests for price calculating for '{ticker}' failed! \nError message: {ex.InnerException?.Message ?? ex.Message}");
                Console.ForegroundColor = ConsoleColor.Gray;
            }

            if (pricesResponse!.Errors.Any() || stockSplitsResponse!.Errors.Any() || pricesResponse.Data!.Count <= 0)
                return Array.Empty<PriceDto>();

            //приводим цену в соответствие для рассчета, если по этому тикеру был сплит

            var pricesResponseItems = pricesResponse.Data.Items;
            var calculatingData = new List<PriceDto>(pricesResponse.Data.Count);

            foreach (var stockSplit in stockSplitsResponse.Data!.Items.OrderByDescending(x => x.Date))
            {
                var splitPrices = pricesResponseItems!.Where(x => x.Date >= stockSplit.Date).ToArray();

                foreach (var price in splitPrices)
                    price.Value *= stockSplit.Divider;

                calculatingData.AddRange(splitPrices);

                pricesResponseItems = pricesResponse.Data.Items.Except(splitPrices, new PriceComparer()).ToArray() as PriceDto[];
            }

            return calculatingData.Union(pricesResponseItems!).ToArray();
        }
    }
}