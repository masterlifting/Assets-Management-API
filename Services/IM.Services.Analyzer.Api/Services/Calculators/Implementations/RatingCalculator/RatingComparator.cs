using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.Services.Calculators.Implementations.RatingCalculator
{
    public static class RatingComparator
    {
        public static decimal? GetComparedResult(List<decimal> results, decimal weight)
        {
            decimal? result = null;

            var data = results.Where(x => x != 0);

            if (data.Any())
                result = data.Average() * weight;

            return result;
        }
        public static decimal CompareValues(decimal[] values, CompareType compareType)
        {
            decimal result = 0;

            if (values is null || values.Length == 1)
                return result;

            var smoothedSampling = GetSmoothedSampling(values);
            var compareTypeValue = (short)compareType;

            for (int j = 1; j < smoothedSampling.Length; j++)
            {
                if (compareType == CompareType.desc & (smoothedSampling[j - 1] < 0 || smoothedSampling[j] < 0))
                {
                    result += 0.0001m;
                }
                else if (smoothedSampling[j - 1] != 0)
                {
                    decimal tempResult = (((smoothedSampling[j] - smoothedSampling[j - 1]) / Math.Abs(smoothedSampling[j - 1])) * compareTypeValue);
                    result += tempResult != 0 ? tempResult : 0.0001m;
                }
                else if (smoothedSampling[j - 1] == 0)
                {
                    smoothedSampling[j] += 1;
                    smoothedSampling[j - 1] += 1;

                    result += (((smoothedSampling[j] - smoothedSampling[j - 1]) / Math.Abs(smoothedSampling[j - 1])) * compareTypeValue);

                    smoothedSampling[j] -= 1;
                    smoothedSampling[j - 1] -= 1;
                }
            }

            return result != 0 ? result / (values.Length - 2) : result;

            static decimal[] GetSmoothedSampling(decimal[] sampling)
            {
                var smoothedSampling = new decimal[sampling.Length];

                for (int i = 1; i < sampling.Length; i++)
                    smoothedSampling[i] = (sampling[i] + sampling[i - 1]) * 0.5m;

                return smoothedSampling;
            }
        }
    }
    public enum CompareType : short
    {
        asc = 100,
        desc = -100
    }
}

