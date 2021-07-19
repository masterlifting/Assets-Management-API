using System;

namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class CoefficientDto
    {
        public string Ticker { get; } = null!;
        public string ReportSourceType { get; } = null!;

        public string ReportSource { get; } = null!;
        public int Year { get; }
        public byte Quarter { get; }

        public DateTime UpdateTime { get; }
    }
}
