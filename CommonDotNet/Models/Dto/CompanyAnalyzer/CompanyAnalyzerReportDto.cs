using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    public class CompanyAnalyzerReportDto : ReportIdentity
    {
        public string SourceType { get; init; } = null!;
    }
}
