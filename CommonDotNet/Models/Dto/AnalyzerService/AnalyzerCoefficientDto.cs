using System;

namespace CommonServices.Models.Dto.AnalyzerService
{
    public class AnalyzerCoefficientDto
    {
        public string Ticker { get; set; } = null!;
        public string ReportSourceType { get; set; } = null!;

        public string ReportSource { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
