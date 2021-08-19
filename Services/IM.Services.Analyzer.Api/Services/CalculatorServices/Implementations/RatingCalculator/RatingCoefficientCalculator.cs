using IM.Services.Analyzer.Api.Models.Calculator.Rating;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces.RatingCalculator;
using IM.Services.Analyzer.Api.Settings;
using IM.Services.Analyzer.Api.Settings.Calculator;

using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices.Implementations.RatingCalculator
{
    public class RatingCoefficientCalculator : IRatingComparisionCalculator
    {
        private readonly CoefficientCalculatorModel[] data;
        private readonly CoefficientWeights coefficientWeights;

        private readonly decimal[] profitabilityCollection;
        private readonly decimal[] roaCollection;
        private readonly decimal[] roeCollection;
        private readonly decimal[] peCollection;
        private readonly decimal[] pbCollection;
        private readonly decimal[] debtLoadCollection;
        private readonly decimal[] epsCollection;

        public RatingCoefficientCalculator(CoefficientCalculatorModel[] data, IOptions<ServiceSettings> options)
        {
            this.data = data;
            coefficientWeights = options.Value.CalculatorSettings.RatingWeights.CoefficientWeights;

            profitabilityCollection = data.Select(x => x.Profitability).Where(x => x != default).ToArray();
            roaCollection = data.Select(x => x.Roa).ToArray();
            roeCollection = data.Select(x => x.Roe).ToArray();
            epsCollection = data.Select(x => x.Eps).ToArray();
            peCollection = data.Select(x => x.Pe).ToArray();
            pbCollection = data.Select(x => x.Pb).ToArray();
            debtLoadCollection = data.Select(x => x.DebtLoad).ToArray();
        }
        public decimal? GetPropertiesComparedResult()
        {
            var comparedResults = new List<decimal>(data.GetType().GetProperties().Length)
            {
                RatingComparator.CompareValues(profitabilityCollection, CompareType.asc),
                RatingComparator.CompareValues(roaCollection, CompareType.asc),
                RatingComparator.CompareValues(roeCollection, CompareType.asc),
                RatingComparator.CompareValues(epsCollection, CompareType.asc),

                RatingComparator.CompareValues(peCollection, CompareType.desc),
                RatingComparator.CompareValues(pbCollection, CompareType.desc),
                RatingComparator.CompareValues(debtLoadCollection, CompareType.desc)
            };

            return RatingComparator.GetComparedResult(comparedResults, coefficientWeights.ComparisionWeght);
        }
        public decimal? GetCoefficientsAverage()
        {
            var positiveCollection = new List<decimal>(4) { profitabilityCollection.Average(), roeCollection.Average(), roaCollection.Average(), epsCollection.Average() };

            var peMoreZero = peCollection.Where(x => x > 0).ToList();
            var peLessZero = peCollection.Where(x => x < 0).ToList();
            var pbMoreZero = pbCollection.Where(x => x > 0).ToList();
            var pbLessZero = pbCollection.Where(x => x < 0).ToList();

            decimal peCollectionResult = peLessZero.Count * coefficientWeights.AverageWeght + (peMoreZero.Any() ? peMoreZero.Average() : 0);
            decimal bpCollectionResult = pbLessZero.Count * coefficientWeights.AverageWeght + (pbMoreZero.Any() ? pbMoreZero.Average() : 0);

            var negativeCollection = new List<decimal>(3) { peCollectionResult, bpCollectionResult, debtLoadCollection.Average() };

            var result = (positiveCollection.Sum() - negativeCollection.Sum()) * coefficientWeights.AverageWeght;

            return result != 0 ? result : null;
        }
    }
}
