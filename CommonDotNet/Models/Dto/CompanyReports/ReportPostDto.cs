using System.ComponentModel.DataAnnotations;
using CommonServices.Attributes;
using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyReports
{
    public class ReportPostDto : ReportIdentity
    {
        [Range(1, int.MaxValue)]
        public int Multiplier { get; set; }
        [NotZero]
        public long StockVolume { get; set; }
        public decimal? Revenue { get; init; }
        public decimal? ProfitNet { get; init; }
        public decimal? ProfitGross { get; init; }
        public decimal? CashFlow { get; init; }
        public decimal? Asset { get; init; }
        public decimal? Turnover { get; init; }
        public decimal? ShareCapital { get; init; }
        public decimal? Dividend { get; init; }
        public decimal? Obligation { get; init; }
        public decimal? LongTermDebt { get; init; }
    }
}
