using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    public class AnalyzerReportDto : ReportIdentity
    {
        public string SourceType { get; init; } = null!;
    }
}
