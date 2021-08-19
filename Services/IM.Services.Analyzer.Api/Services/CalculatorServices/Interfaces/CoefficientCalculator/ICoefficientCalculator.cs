using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Models.Dto;

namespace IM.Services.Analyzer.Api.Services.CalculatorServices.Interfaces.CoefficientCalculator
{
    interface ICoefficientCalculator
    {
        Coefficient Calculate(ReportDto report, decimal lastPrice);
    }
}
