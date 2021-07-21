using IM.Services.Analyzer.Api.Models.Calculator.Rating;
using IM.Services.Analyzer.Api.Services.Calculators.Interfaces.RatingCalculator;
using IM.Services.Analyzer.Api.Settings;
using IM.Services.Analyzer.Api.Settings.Calculator;

using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Analyzer.Api.Services.Calculators.Implementations.RatingCalculator
{
    public class RatingReportCalculator : IRatingComparisionCalculator
    {
        private readonly ReportCalculatorModel[] data;
        private readonly ReportWeights reportWeights;

        private readonly decimal[] revenueCollection;
        private readonly decimal[] profitNetCollection;
        private readonly decimal[] profitGrossCollection;
        private readonly decimal[] assetCollection;
        private readonly decimal[] turnoverCollection;
        private readonly decimal[] shareCapitalCollection;
        private readonly decimal[] dividendCollection;
        private readonly decimal[] obligationCollection;
        private readonly decimal[] longTermDebtCollection;
        private readonly decimal[] cashFlowCollection;

        public RatingReportCalculator(ReportCalculatorModel[] data, IOptions<ServiceSettings> options)
        {
            this.data = data;
            reportWeights = options.Value.CalculatorSettings.RatingWeights.ReportWeights;

            revenueCollection = data.Select(x => x.Revenue).ToArray();
            profitNetCollection = data.Select(x => x.ProfitNet).ToArray();
            profitGrossCollection = data.Select(x => x.ProfitGross).ToArray();
            assetCollection = data.Select(x => x.Asset).ToArray();
            turnoverCollection = data.Select(x => x.Turnover).ToArray();
            shareCapitalCollection = data.Select(x => x.ShareCapital).ToArray();
            dividendCollection = data.Select(x => x.Dividend).ToArray();
            obligationCollection = data.Select(x => x.Obligation).ToArray();
            longTermDebtCollection = data.Select(x => x.LongTermDebt).ToArray();
            cashFlowCollection = data.Select(x => x.CashFlow).ToArray();
        }

        public decimal? GetPropertiesComparedResult()
        {
            var comparedResults = new List<decimal>(data.GetType().GetProperties().Length)
            {
                RatingComparator.CompareValues(revenueCollection, CompareType.asc),
                RatingComparator.CompareValues(profitNetCollection, CompareType.asc),
                RatingComparator.CompareValues(profitGrossCollection, CompareType.asc),
                RatingComparator.CompareValues(assetCollection, CompareType.asc),
                RatingComparator.CompareValues(turnoverCollection, CompareType.asc),
                RatingComparator.CompareValues(shareCapitalCollection, CompareType.asc),
                RatingComparator.CompareValues(dividendCollection, CompareType.asc),

                RatingComparator.CompareValues(obligationCollection, CompareType.desc),
                RatingComparator.CompareValues(longTermDebtCollection, CompareType.desc)
            };

            return RatingComparator.GetComparedResult(comparedResults, reportWeights.ComparisionWeght);
        }
        public decimal? GetCashFlowBalance()
        {
            decimal? result = null;

            if (cashFlowCollection.Length > 0)
            {
                decimal stepPercent = reportWeights.CashFlowMaxPercentResult / cashFlowCollection.Length;

                for (int i = 0; i < cashFlowCollection.Length; i++)
                    result = cashFlowCollection[i] > 0 ? result + stepPercent : result - stepPercent;

                result *= reportWeights.CashFlowWeght;
            }

            return result;
        }
    }
}
