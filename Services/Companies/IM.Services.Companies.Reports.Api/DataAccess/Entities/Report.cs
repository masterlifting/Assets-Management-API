using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Services.Companies.Reports.Api.DataAccess.Entities
{
    public class Report
    {
        public virtual ReportSource ReportSource { get; set; }
        public int ReportSourceId { get; set; }
        public int Year { get; set; }
        public byte Quarter { get; set; }

        public long StockVolume { get; set; }

        [Column(TypeName = "Decimal(18,4)")]
        public decimal? Revenue { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? ProfitNet { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? ProfitGross { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal?CashFlow { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? Asset { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? Turnover { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? ShareCapital { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? Dividend { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? Obligation { get; set; }
        [Column(TypeName = "Decimal(18,4)")]
        public decimal? LongTermDebt { get; set; }
    }
}