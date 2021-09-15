using System;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AnalyzerRatingDto
    {
        public string Ticker { get; set; } = null!;
        public DateTime UpdateTime { get; set; }

        public int Place { get; set; }

        public decimal PriceComparison { get; set; }
        public decimal ReportComparison { get; set; }
      
        public decimal Result { get; set; }
    }
}
