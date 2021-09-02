using CommonServices.Models.Entity;

namespace CommonServices.Models.AnalyzerService
{
    public class AnalyzerReportDto : ReportIdentity
    {
        public string TickerName { get; set; } = null!;
    }
}
