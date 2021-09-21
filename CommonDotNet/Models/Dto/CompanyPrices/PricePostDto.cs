using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class PricePostDto : PriceIdentity
    {
        public byte SourceTypeId { get; init; }
        public decimal Value { get; set; }
    }
}
