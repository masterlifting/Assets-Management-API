using CommonServices.Models.Dto;

using IM.Services.Analyzer.Api.Models.Calculator;

using System;
using System.Collections.Generic;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices
{
    public class CoefficientCalculator
    {
        private readonly Dictionary<string, int> multiplicity;
        public CoefficientCalculator()
        {
            multiplicity = new()
            {
                { "INVESTING", 1_000_000 }
            };
        }

        public Coefficient Calculate(ReportDto report, decimal lastPrice)
        {
            if (report is null || lastPrice <= 0)
                throw new ArgumentException($"{nameof(Calculate)} parameters is incorrect");

            decimal stockVolume = report.StockVolume == 0 ? throw new ArgumentNullException($"{nameof(report.StockVolume)} is 0!") : report.StockVolume;
            decimal profitNet = report.ProfitNet ?? throw new ArgumentNullException($"{nameof(report.ProfitNet)} is null!");
            decimal revenue = report.Revenue ?? throw new ArgumentNullException($"{nameof(report.Revenue)} is null!");
            decimal asset = report.Asset ?? throw new ArgumentNullException($"{nameof(report.Asset)} is null!");
            decimal shareCapital = report.ShareCapital ?? throw new ArgumentNullException($"{nameof(report.ShareCapital)} is null!");
            decimal obligation = report.Obligation ?? throw new ArgumentNullException($"{nameof(report.Obligation)} is null!");

            int multi = multiplicity.ContainsKey(report.SourceType.ToUpperInvariant()) ? multiplicity[report.SourceType.ToUpperInvariant()] : 1;

            try
            {
                decimal eps = profitNet * multi / stockVolume;

                return new()
                {
                    Eps = eps,
                    Profitability = ((profitNet / revenue) + (revenue / asset)) / 2,
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
