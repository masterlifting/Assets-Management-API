using CommonServices.Models.Entity;

namespace CommonServices.Models.AnalyzerService
{
    public class AnalyzerPriceDto : PriceIdentity
    {
        public byte PriceSourceTypeId { get; set; }
    }
}
