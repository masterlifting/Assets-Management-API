namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class ReportDto
    {
        public string Ticker { get; set; } = null!;
        public string ReportSourceType { get; } = null!;
        public int Year { get; }
        public byte Quarter { get; }

        public long StockVolume { get; }

        public decimal? Revenue { get; }
        public decimal? ProfitNet { get; }
        public decimal? ProfitGross { get; }
        public decimal? CashFlow { get; }
        public decimal? Asset { get; }
        public decimal? Turnover { get; }
        public decimal? ShareCapital { get; }
        public decimal? Dividend { get; }
        public decimal? Obligation { get; }
        public decimal? LongTermDebt { get; }
    }
}
