using CommonServices.Models.Dto.CompanyReports;

using IM.Service.Company.Analyzer.Models.Calculator;

using System;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices
{
    public class CoefficientCalculator
    {
        public Coefficient Calculate(ReportGetDto report, decimal lastPrice)
        {
            if (report is null || report.Multiplier <= 0 || lastPrice <= 0)
                throw new ArgumentException($"{nameof(Calculate)} parameters is incorrect");

            decimal stockVolume = report.StockVolume == 0 ? throw new ArgumentNullException($"{nameof(report.StockVolume)} is 0!") : report.StockVolume;
            var profitNet = report.ProfitNet ?? throw new ArgumentNullException($"{nameof(report.ProfitNet)} is null!");
            var revenue = report.Revenue ?? throw new ArgumentNullException($"{nameof(report.Revenue)} is null!");
            var asset = report.Asset ?? throw new ArgumentNullException($"{nameof(report.Asset)} is null!");
            var shareCapital = report.ShareCapital ?? throw new ArgumentNullException($"{nameof(report.ShareCapital)} is null!");
            var obligation = report.Obligation ?? throw new ArgumentNullException($"{nameof(report.Obligation)} is null!");

            try
            {
                var eps = profitNet * report.Multiplier / stockVolume;

                return new Coefficient
                {
                    Eps = eps,
                    Profitability = (profitNet / revenue + revenue / asset) * 0.5m,
                    Roa = profitNet / asset * 100,
                    Roe = profitNet / shareCapital * 100,
                    DebtLoad = obligation / asset,
                    Pe = lastPrice / eps,
                    Pb = lastPrice * stockVolume / ((asset - obligation) * report.Multiplier),
                };
            }
            catch
            {
                throw new ArithmeticException("Coefficient calculating error!");
            }
        }
    }
}
