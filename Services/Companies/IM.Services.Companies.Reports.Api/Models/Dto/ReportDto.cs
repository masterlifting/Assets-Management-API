
using IM.Services.Companies.Reports.Api.DataAccess.Entities;

namespace IM.Services.Companies.Reports.Api.Models.Dto
{
    public class ReportDto : CommonServices.Models.Dto.ReportDto
    {
        public ReportDto(Report report, string sourceType, string ticker)
        {
            if (report is not null)
            {
                Ticker = ticker;
                SourceType = sourceType;
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
    }
}