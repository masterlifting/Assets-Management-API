
using IM.Services.Analyzer.Api.Models.Calculator;

using System;
using System.Linq;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public static class RatingComparator
    {
        public static Sample[] CompareSample(Sample[] sample)
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

            int zeroDeviationCount = results.Where(x => x != 0).Count();
            decimal deviationCoefficient = (decimal)zeroDeviationCount / results.Length;
            return results.Sum() * deviationCoefficient;
        }

        static Sample[] ComputeValues(Sample[] sample)
        {
            var results = new Sample[sample.Length];
            results[0] = new()
            {
                Value = 0,
                Index = sample[0].Index,
                CompareType = sample[0].CompareType
            };

            for (int i = 1; i < sample.Length; i++)
                results[i] = new()
                {
                    Value = ComputeDeviationPercent(sample[i - 1].Value, sample[i].Value, sample[i].CompareType),
                    Index = sample[i].Index,
                    CompareType = sample[i].CompareType
                };

            return results;
        }
        static decimal ComputeDeviationPercent(decimal previousValue, decimal nextValue, CompareType compareType) =>
            (nextValue - previousValue) / Math.Abs(previousValue) * ((short)compareType);
    }
}

