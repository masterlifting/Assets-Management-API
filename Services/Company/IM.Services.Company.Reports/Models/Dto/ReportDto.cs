using IM.Services.Company.Reports.DataAccess.Entities;

namespace IM.Services.Company.Reports.Models.Dto
{
    public class ReportDto : CommonServices.Models.Dto.ReportDto
    {
        public ReportDto(Report report, byte sourceTypeId, string sourceType)
        {
            SourceTypeId = sourceTypeId;
            SourceType = sourceType;

            if (report is not null)
            {
                TickerName = report.TickerName;
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