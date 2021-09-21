using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class StockSplitGetDto : PriceIdentity
    {
        public string Company { get; init; } = null!;
        public int Divider { get; init; }
    }
}
