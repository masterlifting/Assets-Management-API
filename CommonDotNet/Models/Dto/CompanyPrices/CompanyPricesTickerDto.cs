using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class CompanyPricesTickerDto : TickerIdentity
    {
        public byte SourceTypeId { get; init; }
    }
}
