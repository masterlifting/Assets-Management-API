using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyReports
{
    public class TickerPostDto : TickerIdentity
    {
        public byte SourceTypeId { get; init; }
        public string? SourceValue { get; init; }
    }
}
