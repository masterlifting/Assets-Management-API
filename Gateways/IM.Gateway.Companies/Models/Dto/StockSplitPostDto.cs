using CommonServices.Models.Entity;

using IM.Gateway.Companies.Services.Attributes;

using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Companies.Models.Dto
{
    public class StockSplitPostDto : PriceIdentity
    {
        [Zero, Range(1, 100)]
        public int Divider { get; init; }
    }
}
