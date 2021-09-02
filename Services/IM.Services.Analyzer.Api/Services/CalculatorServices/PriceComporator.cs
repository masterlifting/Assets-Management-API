using CommonServices.Models.Dto;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Models.Calculator;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class PriceComporator : IAnalyzerComparator<Price>
    {
        private readonly PriceDto[] prices;
        private readonly Sample[] valueCollection;

        public PriceComporator(PriceDto[] prices)
        {
            this.prices = prices;
            valueCollection = new Sample[prices.Length];
            SetData();
        }
        public Price[] GetCoparedSample()
        {
            var comparedSample = RatingComparator.CompareSample(valueCollection);
            var result = new Price[comparedSample.Length];

            for (uint i = 0; i < comparedSample.Length; i++)
                result[i] = new()
                {
                    TickerName = prices[comparedSample[i].Index].TickerName,
                    Date = prices[comparedSample[i].Index].Date,
                    PriceSourceTypeId = prices[comparedSample[i].Index].PriceSourceTypeId,
                    Result = comparedSample[i].Value,
                    StatusId = (byte)StatusType.Calculated
                };

            return result;
        }
        void SetData()
        {
            for (uint i = 0; i < prices.Length; i++)
                valueCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = prices[i].Value };
        }
    }
}
