using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto
{
    public class PriceDto : PriceIdentity
    {
        public string? SourceType { get; set; }
        public byte PriceSourceTypeId { get; set; }
        public decimal Value { get; set; }
    }
}
