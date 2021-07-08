using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Coefficient
    {
        [Required, StringLength(300)]
        public string ReportSource { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }


        public string TickerName { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;

        [StringLength(50)]
        public string ReportSourceType { get; set; } = null!;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "Decimal(18,2)")]
        public decimal Pe { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Pb { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal DebtLoad { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Profitability { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Roa { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Roe { get; set; }
        [Column(TypeName = "Decimal(18,2)")]
        public decimal Eps { get; set; }
    }
}
