using CommonServices.Models.Entity;

using System.ComponentModel.DataAnnotations.Schema;

namespace IM.Service.Company.Reports.DataAccess.Entities
{
    public class Report : ReportIdentity
    {
        public virtual Ticker Ticker { get; set; }

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