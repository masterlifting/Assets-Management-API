
using IM.Services.Analyzer.Api.Models.Calculator;

using System;
using System.Collections.Generic;
using System.Linq;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public static class RatingComparator
    {
        public static Sample[] CompareSample(IEnumerable<Sample> sample)
        {
            var results = Array.Empty<Sample>();
            var cleanedValues = sample.Where(x => x.Value != 0).ToArray();

            if (cleanedValues.Length >= 2)
                results = ComputeValues(cleanedValues);

            return results;
        }
        public static decimal ComputeSampleResult(decimal[] results)
        {
            if (!results.Any())
                return 0;

            var zeroDeviationCount = results.Count(x => x != 0);
            var deviationCoefficient = (decimal)zeroDeviationCount / results.Length;
            return results.Sum() * deviationCoefficient;
        }

        private static Sample[] ComputeValues(IReadOnlyList<Sample> sample)
        {
            var results = new Sample[sample.Count];
            results[0] = new Sample
            {
                Value = 0,
                Index = sample[0].Index,
                CompareType = sample[0].CompareType
            };

            for (var i = 1; i < sample.Count; i++)
                results[i] = new Sample
                {
                    Value = ComputeDeviationPercent(sample[i - 1].Value, sample[i].Value, sample[i].CompareType),
                    Index = sample[i].Index,
                    CompareType = sample[i].CompareType
                };

            return results;
        }
        private static decimal ComputeDeviationPercent(decimal previousValue, decimal nextValue, CompareType compareType) =>
            (nextValue - previousValue) / Math.Abs(previousValue) * (short)compareType;
    }
}

