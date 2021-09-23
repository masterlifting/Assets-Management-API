using CommonServices.Attributes;

using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Dto.CompanyReports
{
    public class ReportPutDto
    {
        [Required, StringLength(50, MinimumLength = 3)]
        public string SourceType { get; init; } = null!;

        [NotZero(nameof(Multiplier))]
        public int Multiplier { get; set; }
        
        [NotZero(nameof(StockVolume))]
        public long StockVolume { get; init; }
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
