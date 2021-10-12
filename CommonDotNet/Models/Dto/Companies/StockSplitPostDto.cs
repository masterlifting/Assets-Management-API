using CommonServices.Attributes;
using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.Companies
{
    public class StockSplitPostDto : PriceIdentity
    {
        [NotZero(nameof(Value))]
        public int Value { get; init; }
    }
}
