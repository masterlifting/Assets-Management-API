using IM.Services.Analyzer.Api.Services.Calculators.Interfaces.RatingCalculator;
using IM.Services.Analyzer.Api.Settings;
using IM.Services.Analyzer.Api.Settings.Calculator;

using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace IM.Services.Analyzer.Api.Services.Calculators.Implementations.RatingCalculator
{
    public class RatingPriceCalculator : IRatingComparisionCalculator
    {
        private readonly decimal[] prices;
        private readonly PriceWeights priceWeights;

        public RatingPriceCalculator(decimal[] prices, IOptions<ServiceSettings> options)
        {
            this.prices = prices;
            priceWeights = options.Value.CalculatorSettings.RatingWeights.PriceWeights;
        }
        public decimal? GetPropertiesComparedResult()
        {
            var comparedResults = new List<decimal>(1)
            {
                RatingComparator.CompareValues(prices, CompareType.asc)
            };

            return RatingComparator.GetComparedResult(comparedResults, priceWeights.ComparisionWeght);
        }
    }
}
