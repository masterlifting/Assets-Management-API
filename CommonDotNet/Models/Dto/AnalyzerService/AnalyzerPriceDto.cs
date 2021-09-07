using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.AnalyzerService
{
    public class AnalyzerPriceDto : PriceIdentity
    {
        public string SourceType { get; set; } = null!;
    }
}
