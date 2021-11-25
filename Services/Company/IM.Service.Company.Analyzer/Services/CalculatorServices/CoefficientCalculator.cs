using IM.Service.Common.Net.Models.Dto.Http.Companies;
using IM.Service.Company.Analyzer.Models.Calculator;

using System;

namespace IM.Service.Company.Analyzer.Services.CalculatorServices;

public class CoefficientCalculator
{
    public Coefficient Calculate(ReportGetDto report, PriceGetDto price)
    {
        if (report.Multiplier <= 0 || price.StockVolume is null or <= 0)
            throw new ArgumentException($"{nameof(Calculate)} parameters is incorrect");

        var profitNet = report.ProfitNet ?? throw new ArgumentNullException($"{nameof(report.ProfitNet)} is null!");
        var revenue = report.Revenue ?? throw new ArgumentNullException($"{nameof(report.Revenue)} is null!");
        var asset = report.Asset ?? throw new ArgumentNullException($"{nameof(report.Asset)} is null!");
        var shareCapital = report.ShareCapital ?? throw new ArgumentNullException($"{nameof(report.ShareCapital)} is null!");
        var obligation = report.Obligation ?? throw new ArgumentNullException($"{nameof(report.Obligation)} is null!");

        try
        {
            var eps = profitNet * report.Multiplier / price.StockVolume.Value;

            return new()
            {
                Eps = eps,
                Profitability = (profitNet / revenue + revenue / asset) * 0.5m,
                Roa = profitNet / asset * 100,
                Roe = profitNet / shareCapital * 100,
                DebtLoad = obligation / asset,
                Pe = price.Value / eps,
                Pb = price.Value * price.StockVolume.Value / ((asset - obligation) * report.Multiplier),
            };
        }
        catch
        {
            throw new ArithmeticException("Coefficient calculating error!");
        }
    }
}