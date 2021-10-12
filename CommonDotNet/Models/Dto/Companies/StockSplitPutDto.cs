using CommonServices.Attributes;

namespace CommonServices.Models.Dto.Companies
{
    public class StockSplitPutDto
    {
        [NotZero(nameof(Value))]
        public int Value { get; init; }
    }
}
