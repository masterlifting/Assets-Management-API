using IM.Services.Analyzer.Api.DataAccess.Entities;
using IM.Services.Analyzer.Api.Models.Calculator.Rating;
using System;

namespace IM.Services.Analyzer.Api.Models.Dto
{
    public class CoefficientDto : CoefficientCalculatorModel
    {
        public CoefficientDto() { }
        public CoefficientDto(Coefficient coefficient)
        {
            if (coefficient is null)
                throw new NullReferenceException($"{nameof(coefficient)} is null");

            Ticker = coefficient.TickerName;
            ReportSourceType = coefficient.ReportSourceType;
            
            ReportSource = coefficient.ReportSource;
            Year = coefficient.Year;
            Quarter = coefficient.Quarter;

            Pe = coefficient.Pe;
            Pb = coefficient.Pb;
            DebtLoad = coefficient.DebtLoad;
            Profitability = coefficient.Profitability;
            Roa = coefficient.Roa;
            Roe = coefficient.Roe;
            Eps = coefficient.Eps;
        }

        public string Ticker { get; } = null!;
        public string ReportSourceType { get; } = null!;
        
        public string ReportSource { get; } = null!;
        public int Year { get; }
        public byte Quarter { get; }

        public DateTime UpdateTime { get; }
    }
}
