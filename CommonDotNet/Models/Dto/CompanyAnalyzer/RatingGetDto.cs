using System;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class RatingGetDto
    {
        public string Ticker { get; init; } = null!;
        public DateTime UpdateTime { get; init; }

        public int Place { get; init; }

        public decimal PriceComparison { get; init; }
        public decimal ReportComparison { get; init; }
      
        public decimal Result { get; init; }
    }
}
