using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompaniesReportsService
{
    public class CompaniesReportsTickerDto : TickerIdentity
    {
        public byte SourceTypeId { get; set; }
        public string? SourceValue { get; set; }
    }
}
