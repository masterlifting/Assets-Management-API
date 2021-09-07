using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.AnalyzerService
{
    public class AnalyzerReportDto : ReportIdentity
    {
        public string SourceType { get; set; } = null!;
    }
}
