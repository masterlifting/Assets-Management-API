using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Models.Dto;
using IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces.CoefficientCalculator;
using System;
using System.Collections.Generic;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices.Implementations.CoefficientCalculator
{
    public class CoefficientCalculator : ICoefficientCalculator
    {
        private readonly Dictionary<string, int> multiplicity;
        public CoefficientCalculator()
        {
            multiplicity = new()
            {
                { "investing", 1_000_000 }
            };
        }

        public Coefficient Calculate(ReportDto report, decimal lastPrice)
        {
            if (report is null || report.StockVolume == 0 || lastPrice <= 0)
                throw new ArgumentException("Coefficient calculator parameters is invalid");

            int multi = multiplicity.ContainsKey(report.ReportSourceType) ? multiplicity[report.ReportSourceType] : 1;

            try
            {
                decimal eps = (report.ProfitNet * multi) / report.StockVolume;
                return new()
                {
                    Eps = eps,
                    Profitability = ((report.ProfitNet / report.Revenue) + (report.Revenue / report.Asset)) / 2,
                    Roa = (report.ProfitNet / report.Asset) * 100,
                    Roe = (report.ProfitNet / report.ShareCapital) * 100,
                    DebtLoad = report.Obligation / report.Asset,
                    Pe = lastPrice / eps,
                    Pb = (lastPrice * report.StockVolume) / ((report.Asset - report.Obligation) * multi),

                    TickerName = report.Ticker,
                    ReportSourceType = report.ReportSourceType,

                    ReportSource = report.ReportSource,
                    Quarter = report.Quarter,
                    Year = report.Year
                };
            }
            catch
            {
                throw new ArithmeticException("Calculating error");
            }
        }
    }
}
