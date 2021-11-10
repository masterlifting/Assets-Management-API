using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Common.Net.RepositoryService.Comparators;

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

using static IM.Service.Common.Net.CommonEnums;
using static IM.Service.Common.Net.HttpServices.QueryStringBuilder;
using static IM.Service.Company.Analyzer.Enums;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class PriceCalculator : IAnalyzerCalculator<Price>
    {
        private readonly RepositorySet<Price> priceRepository;
        private readonly CompanyDataClient dataClient;

        public PriceCalculator(
            RepositorySet<Price> priceRepository,
            CompanyDataClient dataClient)
        {
            this.priceRepository = priceRepository;
            this.dataClient = dataClient;
        }

        public async Task<bool> IsSetCalculatingStatusAsync(Price[] prices, string info)
        {
            foreach (var price in prices)
                price.StatusId = (byte)StatusType.Calculating;

            return (await priceRepository.UpdateAsync(prices, $"price calculating status for '{info}'")).error is not null;
        }
        public async Task<bool> CalculateAsync()
        {
            var prices = await priceRepository.GetSampleAsync(x =>
                x.StatusId == (byte)StatusType.ToCalculate
                || x.StatusId == (byte)StatusType.CalculatedPartial
                || x.StatusId == (byte)StatusType.Error);

            if (!prices.Any())
                return false;

            foreach (var group in prices.GroupBy(x => x.CompanyId))
                await BaseCalculateAsync(group);

            return true;
        }

        private async Task BaseCalculateAsync(IGrouping<string, Price> group)
        {
            var companyId = group.Key;
            
            var orderedPrices = group.OrderBy(x => x.Date).ToArray();
            var dateStart = orderedPrices[0].Date.AddMonths(-1);

            if (!await IsSetCalculatingStatusAsync(orderedPrices, companyId))
                throw new DataException($"set price calculating status for '{companyId}' failed");

            try
            {
                var calculateResult = await GetCalculatedResultAsync(companyId, dateStart);
                await priceRepository.CreateUpdateAsync(calculateResult, new CompanyDateComparer<Price>(), $"price calculated result for {companyId}");
            }
            catch (Exception exception)
            {
                foreach (var price in orderedPrices)
                    price.StatusId = (byte)StatusType.Error;

                await priceRepository.CreateUpdateAsync(orderedPrices, new CompanyDateComparer<Price>(), $"calculate prices for {companyId}");

                throw new ArithmeticException($"price calculated for '{companyId}' failed! \nError: {exception.Message}");
            }
        }
        private async Task<Price[]> GetCalculatedResultAsync(string companyId, DateTime dateStart)
        {
            var calculatingData = await GetCalculatingDataAsync(companyId, dateStart);

            if (!calculatingData.Any())
                throw new DataException($"prices for '{companyId}' not found!");

            var calculator = new PriceComparator(calculatingData);
            return calculator.GetComparedSample();
        }
        private async Task<IReadOnlyCollection<PriceGetDto>> GetCalculatingDataAsync(string companyId, DateTime date)
        {
            var pricesResponse = await dataClient.Get<PriceGetDto>(
                "prices",
                GetQueryString(HttpRequestFilterType.More, companyId, date.Year, date.Month, date.Day),
                new(1, int.MaxValue));

            return pricesResponse.Errors.Any()
                ? throw new BadHttpRequestException(string.Join('\n', pricesResponse.Errors))
                : pricesResponse.Data!.Items;
        }
    }
}