using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyAnalyzer
{
    public class CompanyAnalyzerPriceDto : PriceIdentity
    {
        public string SourceType { get; set; } = null!;
    }
}
