using CommonServices.Attributes;

namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class StockSplitPutDto
    {
        [NotZero(nameof(Value))]
        public int Value { get; init; }
    }
}
