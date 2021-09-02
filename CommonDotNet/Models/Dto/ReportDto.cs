using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto
{
    public abstract class ReportDto : ReportIdentity
    {
        public string Ticker { get; set; } = null!;
        public string SourceType { get; set; } = null!;

        public long StockVolume { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? ProfitNet { get; set; }
        public decimal? ProfitGross { get; set; }
        public decimal? CashFlow { get; set; }
        public decimal? Asset { get; set; }
        public decimal? Turnover { get; set; }
        public decimal? ShareCapital { get; set; }
        public decimal? Dividend { get; set; }
        public decimal? Obligation { get; set; }
        public decimal? LongTermDebt { get; set; }
    }
}
