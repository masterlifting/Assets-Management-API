using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class TickerPostDto : TickerIdentity
    {
        public byte SourceTypeId { get; set; }
    }
}
