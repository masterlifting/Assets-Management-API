using System;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CompanyAnalyzerCoefficientDto
    {
        public string Ticker { get; set; } = null!;
        public string ReportSourceType { get; set; } = null!;

        public string ReportSource { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}
