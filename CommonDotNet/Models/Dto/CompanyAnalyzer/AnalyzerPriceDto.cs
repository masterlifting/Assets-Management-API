using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    public class AnalyzerPriceDto : PriceIdentity
    {
        public string SourceType { get; set; } = null!;
    }
}
