using CommonServices;
using CommonServices.Models.Dto.CompanyPrices;
using CommonServices.Models.Dto.GatewayCompanies;
using CommonServices.Models.Entity;

using IM.Service.Company.Analyzer.Clients;
using IM.Service.Company.Analyzer.DataAccess.Entities;
using IM.Service.Company.Analyzer.DataAccess.Repository;
using IM.Service.Company.Analyzer.Services.CalculatorServices.Interfaces;

using Microsoft.AspNetCore.Http;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

using static CommonServices.CommonEnums;
using static CommonServices.HttpServices.QueryStringBuilder;
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

        public async Task<bool> IsSetCalculatingStatusAsync(Price[] prices, string info)
        {
            foreach (var price in prices)
                price.StatusId = (byte)StatusType.Calculating;

            var (errors, _) = await repository.UpdateAsync(prices, $"price calculating status for '{info}'");

            return !errors.Any();
        }

        public async Task<bool> CalculateAsync()
        {
            var prices = await repository.GetSampleAsync(x =>
                x.StatusId == (byte)StatusType.ToCalculate
                || x.StatusId == (byte)StatusType.CalculatedPartial
                || x.StatusId == (byte)StatusType.Error);

            if (!prices.Any())
                return false;

            foreach (var group in prices.GroupBy(x => x.TickerName))
            {
                var firstElement = group.OrderBy(x => x.Date).First();
                var dateStart = CommonHelper.GetExchangeLastWorkday(firstElement.SourceType, firstElement.Date);

                await BaseCalculateAsync(group, dateStart);
            }

            return true;
        }
        public async Task<bool> CalculateAsync(DateTime dateStart)
        {
            var prices = await repository.GetSampleAsync(x => true);

            if (!prices.Any())
                return false;

            foreach (var group in prices.GroupBy(x => x.TickerName))
            {
                await BaseCalculateAsync(group, dateStart);
            }

            return true;
        }

        private async Task BaseCalculateAsync(IGrouping<string, Price> group, DateTime dateStart)
        {
            var priceGroup = group.ToArray();

            if (!await IsSetCalculatingStatusAsync(priceGroup, group.Key))
                throw new DataException($"set price calculating status for '{group.Key}' failed");

            try
            {
                var calculatingData = await GetCalculatingDataAsync(group.Key, dateStart);

                if (!calculatingData.Any())
                    throw new DataException($"prices for '{group.Key}' not found!");

                var calculator = new PriceComparator(calculatingData);
                await repository.CreateUpdateAsync(
                    calculator.GetComparedSample(),
                    new PriceComparer(),
                    $"price calculated result for {group.Key}");
            }
            catch (Exception ex)
            {
                foreach (var price in group)
                    price.StatusId = (byte)StatusType.Error;

                await repository.CreateUpdateAsync(priceGroup, new PriceComparer(), $"calculate prices for {group.Key}");

                throw new ArithmeticException($"price calculated for '{group.Key}' failed! \nError: {ex.Message}");
            }
        }
        private async Task<IReadOnlyCollection<PriceGetDto>> GetCalculatingDataAsync(string ticker, DateTime date)
        {
            var stockSplitsResponse = await gatewayClient.Get<StockSplitGetDto>(
                "api/stocksplits",
                GetQueryString(HttpRequestFilterType.More, ticker, date.Year, date.Month, date.Day),
                new(1, int.MaxValue));

            if (stockSplitsResponse.Errors.Any())
                throw new BadHttpRequestException(string.Join('\n', stockSplitsResponse.Errors));

            var pricesResponse = await pricesClient.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, ticker, date.Year, date.Month, date.Day),
                new(1, int.MaxValue));

            if (pricesResponse.Errors.Any())
                throw new BadHttpRequestException(string.Join('\n', pricesResponse.Errors));

            //приводим цену в соответствие для рассчета, если по этому тикеру был сплит
            var pricesResponseItems = pricesResponse.Data!.Items;
            var splittedPriceResult = new List<PriceGetDto>(pricesResponse.Data.Count);

            foreach (var stockSplit in stockSplitsResponse.Data!.Items.OrderByDescending(x => x.Date))
            {
                var splittedPrices = pricesResponseItems.Where(x => x.Date >= stockSplit.Date).ToArray();

                foreach (var price in splittedPrices)
                    price.Value *= stockSplit.Divider;

                splittedPriceResult.AddRange(splittedPrices);

                var exceptedResult = pricesResponseItems.Except(splittedPrices, new PriceComparer()).ToArray();

                pricesResponseItems = pricesResponse.Data.Items.Join(exceptedResult, x => (x.TickerName, x.Date),
                    y => (y.TickerName, y.Date), (x, _) => x).ToArray();
            }

            return splittedPriceResult.Union(pricesResponseItems!).ToArray();
        }
    }
}