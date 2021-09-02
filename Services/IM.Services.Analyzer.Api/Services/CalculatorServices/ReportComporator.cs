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
        private readonly ReportDto[] reportsDto;
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
        public ReportComporator(ReportDto[] reportsDto, PriceDto[]? prices)
        {
            this.reportsDto = reportsDto;

            revenueCollection = new Sample[reportsDto.Length];
            profitNetCollection = new Sample[reportsDto.Length];
            profitGrossCollection = new Sample[reportsDto.Length];
            assetCollection = new Sample[reportsDto.Length];
            turnoverCollection = new Sample[reportsDto.Length];
            shareCapitalCollection = new Sample[reportsDto.Length];
            dividendCollection = new Sample[reportsDto.Length];
            obligationCollection = new Sample[reportsDto.Length];
            longTermDebtCollection = new Sample[reportsDto.Length];
            cashFlowCollection = new Sample[reportsDto.Length];

            if (prices is not null && prices.Any())
            {
                coefficientCalculator = new();

                pe = new Sample[reportsDto.Length];
                pb = new Sample[reportsDto.Length];
                debtLoad = new Sample[reportsDto.Length];
                profitability = new Sample[reportsDto.Length];
                roa = new Sample[reportsDto.Length];
                roe = new Sample[reportsDto.Length];
                eps = new Sample[reportsDto.Length];

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
        public Report[] GetCoparedSample()
        {
            var result = new Report[samples.Length * reportsDto.Length];
            byte statusId = samples.Length == 17 ? (byte)StatusType.Calculated : (byte)StatusType.CalculatedPartial;

            for (uint i = 0; i < samples.Length; i++)
            {
                var comparedSample = RatingComparator.CompareSample(samples[i]);

                for (uint j = 0; j < comparedSample.Length; j++)
                    result[i] = new()
                    {
                        TickerName = reportsDto[comparedSample[j].Index].Ticker,
                        ReportSourceId = reportsDto[comparedSample[j].Index].ReportSourceId,
                        Year = reportsDto[comparedSample[j].Index].Year,
                        Quarter = reportsDto[comparedSample[j].Index].Quarter,
                        Result = comparedSample[j].Value,
                        StatusId = statusId
                    };
            }

            return result;
        }
        void SetData(PriceDto[]? prices)
        {
            for (uint i = 0; i < reportsDto.Length; i++)
            {
                revenueCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].Revenue.HasValue ? reportsDto[i].Revenue!.Value : 0 };
                profitNetCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].ProfitNet.HasValue ? reportsDto[i].ProfitNet!.Value : 0 };
                profitGrossCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].ProfitGross.HasValue ? reportsDto[i].ProfitGross!.Value : 0 };
                assetCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].Asset.HasValue ? reportsDto[i].Asset!.Value : 0 };
                turnoverCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].Turnover.HasValue ? reportsDto[i].Turnover!.Value : 0 };
                shareCapitalCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].ShareCapital.HasValue ? reportsDto[i].ShareCapital!.Value : 0 };
                dividendCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].Dividend.HasValue ? reportsDto[i].Dividend!.Value : 0 };
                cashFlowCollection[i] = new() { Index = i, CompareType = CompareType.Asc, Value = reportsDto[i].CashFlow.HasValue ? reportsDto[i].CashFlow!.Value : 0 };
                obligationCollection[i] = new() { Index = i, CompareType = CompareType.Desc, Value = reportsDto[i].Obligation.HasValue ? reportsDto[i].Obligation!.Value : 0 };
                longTermDebtCollection[i] = new() { Index = i, CompareType = CompareType.Desc, Value = reportsDto[i].LongTermDebt.HasValue ? reportsDto[i].LongTermDebt!.Value : 0 };

                if (prices is not null && prices.Any())
                {
                    var coefficients = coefficientCalculator!.Calculate(reportsDto[i], prices.Max(x => x.Value));

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
