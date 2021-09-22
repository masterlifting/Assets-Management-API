using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyReports
{
    public class ReportGetDto : ReportIdentity
    {
        public string SourceType { get; init; } = null!;
        public int Multiplier { get; init; }
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
