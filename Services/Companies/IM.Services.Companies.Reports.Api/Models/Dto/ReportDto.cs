using IM.Services.Companies.Reports.Api.DataAccess.Entities;

namespace IM.Services.Companies.Reports.Api.Models.Dto
{
    public class ReportDto
    {
        public ReportDto() { }
        public ReportDto(Report report, string sourceType, string ticker)
        {
            if (report is not null)
            {
                Ticker = ticker;
                ReportSourceType = sourceType;
                Year = report.Year;
                Quarter = report.Quarter;
                StockVolume = report.StockVolume;
                Revenue = report.Revenue;
                ProfitNet = report.ProfitNet;
                ProfitGross = report.ProfitGross;
                CashFlow = report.CashFlow;
                Asset = report.Asset;
                Turnover = report.Turnover;
                ShareCapital = report.ShareCapital;
                Dividend = report.Dividend;
                Obligation = report.Obligation;
                LongTermDebt = report.LongTermDebt;
            }
        }
        public string Ticker { get; set; }
        public string ReportSourceType { get; }
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