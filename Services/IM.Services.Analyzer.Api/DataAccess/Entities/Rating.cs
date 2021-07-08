using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Rating
    {
        [Key]
        public int Place { get; set; }

        public string TickerName { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        public decimal Result { get; set; }

        public decimal? PriceComparison { get; set; }
        public decimal? ReportComparison { get; set; }
        public decimal? CashFlowPositiveBalance { get; set; }
        public decimal? CoefficientComparison { get; set; }
        public decimal? CoefficientAverage { get; set; }
    }
}
