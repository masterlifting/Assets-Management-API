using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Services.Company.Analyzer.DataAccess.Entities
{
    public class Rating
    {
        [Key]
        public int Place { get; set; }

        public string TickerName { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "Decimal(18,2)")]
        public decimal Result { get; set; }

        [Column(TypeName = "Decimal(18,4)")]
        public decimal PriceComparison { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal ReportComparison { get; set; }
    }
}
