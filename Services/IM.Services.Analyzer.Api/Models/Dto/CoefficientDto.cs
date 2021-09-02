using CommonServices.Models.Entity;

using IM.Services.Analyzer.Api.Models.Calculator;

using System;

namespace IM.Services.Analyzer.Api.Models.Dto
{
    public class CoefficientDto : Coefficient
    {
        public CoefficientDto() { }
        public CoefficientDto(ReportIdentity report, Coefficient coefficient)
        {
            if (coefficient is null)
                throw new NullReferenceException($"{nameof(coefficient)} is null");

            Year = report.Year;
            Quarter = report.Quarter;

            Pe = coefficient.Pe;
            Pb = coefficient.Pb;
            DebtLoad = coefficient.DebtLoad;
            Profitability = coefficient.Profitability;
            Roa = coefficient.Roa;
            Roe = coefficient.Roe;
            Eps = coefficient.Eps;
        }

        public int Year { get; }
        public byte Quarter { get; }
    }
}
