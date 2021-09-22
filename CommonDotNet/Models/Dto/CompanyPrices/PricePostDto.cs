
using CommonServices.Attributes;
using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class PricePostDto : PriceIdentity
    {
        [NotZero]
        public decimal Value { get; init; }
    }
}
