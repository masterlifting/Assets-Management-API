using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.Companies
{
    public class StockSplitGetDto : PriceIdentity
    {
        public string Company { get; init; } = null!;
        public int Divider { get; init; }
        public int Value { get; init; }
    }
}
