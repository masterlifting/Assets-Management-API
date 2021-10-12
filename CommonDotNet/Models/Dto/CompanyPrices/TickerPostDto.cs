using System.ComponentModel.DataAnnotations;
using CommonServices.Models.Entity;

namespace CommonServices.Models.Dto.CompanyPrices
{
    public class TickerPostDto : TickerIdentity
    {
        [Range(1, byte.MaxValue)]
        public byte SourceTypeId { get; init; }
        public string? SourceValue { get; init; }
    }
}
