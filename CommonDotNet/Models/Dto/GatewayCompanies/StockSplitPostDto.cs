using CommonServices.Models.Entity;

using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class StockSplitPostDto : PriceIdentity
    {
        [Range(1, 100)]
        public int Divider { get; init; }
    }
}
