using CommonServices.Models.Dto;

using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Models.Calculator;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces;

using System.Linq;

using static IM.Services.Analyzer.Api.Enums;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class ReportComporator : IAnalyzerComparator<Report>
    {
        private readonly ReportDto[] reports;
        private readonly Sample[][] samples;

        private readonly Sample[] revenueCollection;
        private readonly Sample[] profitNetCollection;
        private readonly Sample[] profitGrossCollection;
        private readonly Sample[] assetCollection;
        private readonly Sample[] turnoverCollection;
        private readonly Sample[] shareCapitalCollection;
        private readonly Sample[] dividendCollection;
        private readonly Sample[] obligationCollection;
        private readonly Sample[] longTermDebtCollection;
        private readonly Sample[] cashFlowCollection;

        private readonly CoefficientCalculator? coefficientCalculator;
        private readonly Sample[]? pe;
        private readonly Sample[]? pb;
        private readonly Sample[]? debtLoad;
        private readonly Sample[]? profitability;
        private readonly Sample[]? roa;
        private readonly Sample[]? roe;
        private readonly Sample[]? eps;
        public ReportComporator(ReportDto[] reports, PriceDto[]? prices)
        {
            this.reports = reports;

            revenueCollection = new Sample[reports.Length];
            profitNetCollection = new Sample[reports.Length];
            profitGrossCollection = new Sample[reports.Length];
            assetCollection = new Sample[reports.Length];
            turnoverCollection = new Sample[reports.Length];
            shareCapitalCollection = new Sample[reports.Length];
            dividendCollection = new Sample[reports.Length];
            obligationCollection = new Sample[reports.Length];
            longTermDebtCollection = new Sample[reports.Length];
            cashFlowCollection = new Sample[reports.Length];

            if (prices is not null && prices.Any())
            {
                coefficientCalculator = new();

                pe = new Sample[reports.Length];
                pb = new Sample[reports.Length];
                debtLoad = new Sample[reports.Length];
                profitability = new Sample[reports.Length];
                roa = new Sample[reports.Length];
                roe = new Sample[reports.Length];
                eps = new Sample[reports.Length];

                samples = new Sample[17][]
                {
                    revenueCollection,
                    profitNetCollection,
                    profitGrossCollection,
                    assetCollection,
                    turnoverCollection,
                    shareCapitalCollection,
                    dividendCollection,
                    obligationCollection,
                    longTermDebtCollection,
                    cashFlowCollection,
                    pe,
                    pb,
                    debtLoad,
                    profitability,
                    roa,
                    roe,
                    eps
                };
            }
            else
                samples = new Sample[10][]
                {
                    revenueCollection,
                    profitNetCollection,
                    profitGrossCollection,
                    assetCollection,
                    turnoverCollection,
                    shareCapitalCollection,
                    dividendCollection,
                    obligationCollection,
                    longTermDebtCollection,
                    cashFlowCollection
                };

            SetData(prices);
        }
        public Report[] GetComparedSample()
        {
            var result = new Report[reports.Length];
            var comparedSample = new Sample[samples.Length][];
            byte statusId = samples.Length == 17 ? (byte)StatusType.Calculated : (byte)StatusType.CalculatedPartial;

            for (uint i = 0; i < samples.Length; i++)
                comparedSample[i] = RatingComparator.CompareSample(samples[i]);

            for (uint i = 0; i < reports.Length; i++)
            {
                var results = new decimal[comparedSample.Length];

                for (uint j = 0; j < comparedSample.Length; j++)
                    for (uint k = 0; k < comparedSample[j].Length; k++)
                        if (comparedSample[j][k].Index == i)
                        {
                            results[j] = comparedSample[j][k].Value;
                            break;
                        }

                result[i] = new()
                {
                    TickerName = reports[i].TickerName,
                    Year = reports[i].Year,
                    Quarter = reports[i].Quarter,
                    SourceTypeId = reports[i].SourceTypeId,
                    Result = RatingComparator.ComputeSampleResult(results),
                    StatusId = statusId
                };
            }
            return result;
        }
        void SetData(PriceDto[]? prices)
        {
            for (uint i = 0; i < reports.Length; i++)
            {
                revenueCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].Revenue.HasValue ? reports[i].Revenue!.Value : 0 };
                profitNetCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].ProfitNet.HasValue ? reports[i].ProfitNet!.Value : 0 };
                profitGrossCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].ProfitGross.HasValue ? reports[i].ProfitGross!.Value : 0 };
                assetCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].Asset.HasValue ? reports[i].Asset!.Value : 0 };
                turnoverCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].Turnover.HasValue ? reports[i].Turnover!.Value : 0 };
                shareCapitalCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].ShareCapital.HasValue ? reports[i].ShareCapital!.Value : 0 };
                dividendCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].Dividend.HasValue ? reports[i].Dividend!.Value : 0 };
                cashFlowCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reports[i].CashFlow.HasValue ? reports[i].CashFlow!.Value : 0 };
                obligationCollection[i] = new() { Index = i, CompareType = CompareType.Desc, Value = reports[i].Obligation.HasValue ? reports[i].Obligation!.Value : 0 };
                longTermDebtCollection[i] = new() { Index = i, CompareType = CompareType.Desc, Value = reports[i].LongTermDebt.HasValue ? reports[i].LongTermDebt!.Value : 0 };
            }
            if (prices is not null && prices.Any())
            {
                for (uint i = 0; i < reports.Length; i++)
                {
                    var coefficients = coefficientCalculator!.Calculate(reports[i], prices.Max(x => x.Value));

                    pe![i] = new() { Index = i, CompareType = CompareType.Desc, Value = coefficients.Pe };
                    pb![i] = new() { Index = i, CompareType = CompareType.Desc, Value = coefficients.Pb };
                    debtLoad![i] = new() { Index = i, CompareType = CompareType.Desc, Value = coefficients.DebtLoad };
                    profitability![i] = new() { Index = i, CompareType = CompareType.Asc, Value = coefficients.Profitability };
                    roa![i] = new() { Index = i, CompareType = CompareType.Asc, Value = coefficients.Roa };
                    roe![i] = new() { Index = i, CompareType = CompareType.Asc, Value = coefficients.Roe };
                    eps![i] = new() { Index = i, CompareType = CompareType.Asc, Value = coefficients.Eps };
                }
            }
        }
    }
}
