
using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class PriceGetDto : PriceIdentity
    {
        public string SourceType { get; init; } = null!;
        public decimal Value { get; set; }
    }
}
