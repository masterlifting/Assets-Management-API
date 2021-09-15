using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class CompaniesPricesTickerDto : TickerIdentity
    {
        public byte SourceTypeId { get; init; }
    }
}
