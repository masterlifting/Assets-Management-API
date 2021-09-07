using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto
{
    public class PriceDto : PriceIdentity
    {
        public string SourceType { get; set; } = null!;
        public byte SourceTypeId { get; set; }

        public decimal Value { get; set; }
    }
}
