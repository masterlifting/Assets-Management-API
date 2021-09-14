using CommonServices.Models.Dto;
using System;
using System.Collections.Generic;
using IM.Service.Company.Analyzer.Models.Calculator;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class CoefficientCalculator
    {
        private readonly Dictionary<string, int> multiplicity;
        public CoefficientCalculator()
        {
            multiplicity = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "investing", 1_000_000 }
            };
        }

        public Coefficient Calculate(ReportDto report, decimal lastPrice)
        {
            if (report is null || lastPrice <= 0)
                throw new ArgumentException($"{nameof(Calculate)} parameters is incorrect");

            decimal stockVolume = report.StockVolume == 0 ? throw new ArgumentNullException($"{nameof(report.StockVolume)} is 0!") : report.StockVolume;
            var profitNet = report.ProfitNet ?? throw new ArgumentNullException($"{nameof(report.ProfitNet)} is null!");
            var revenue = report.Revenue ?? throw new ArgumentNullException($"{nameof(report.Revenue)} is null!");
            var asset = report.Asset ?? throw new ArgumentNullException($"{nameof(report.Asset)} is null!");
            var shareCapital = report.ShareCapital ?? throw new ArgumentNullException($"{nameof(report.ShareCapital)} is null!");
            var obligation = report.Obligation ?? throw new ArgumentNullException($"{nameof(report.Obligation)} is null!");

            var multi = multiplicity.ContainsKey(report.SourceType) ? multiplicity[report.SourceType] : 1_000_000;

            try
            {
                var eps = profitNet * multi / stockVolume;

                return new Coefficient
                {
                    Eps = eps,
                    Profitability = (profitNet / revenue + revenue / asset) * 0.5m,
                    Roa = profitNet / asset * 100,
                    Roe = profitNet / shareCapital * 100,
                    DebtLoad = obligation / asset,
                    Pe = lastPrice / eps,
                    Pb = lastPrice * stockVolume / ((asset - obligation) * multi),
                };
            }
            catch
            {
                throw new ArithmeticException("Coefficient calculating error!");
            }
        }
    }
}
