using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompaniesPricesService
{
    public class CompaniesPricesTickerDto : TickerIdentity
    {
        public byte SourceTypeId { get; set; }
    }
}
