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
        public Price[] GetComparedSample()
        {
            var comparedSample = RatingComparator.CompareSample(valueCollection);
            var result = new Price[comparedSample.Length];

            for (int i = 0; i < prices.Length; i++)
                for (uint j = 0; j < comparedSample.Length; j++)
                    if (comparedSample[j].Index == i)
                    {
                        result[j] = new()
                        {
                            TickerName = prices[i].TickerName,
                            Date = prices[i].Date,
                            SourceTypeId = prices[i].SourceTypeId,
                            Result = comparedSample[j].Value,
                            StatusId = (byte)StatusType.Calculated
                        };

                        break;
                    }

            return result;
        }
        void SetData()
        {
            for (uint i = 0; i < prices.Length; i++)
                valueCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = prices[i].Value };
        }
    }
}
