
using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class CompanyPostDto
    {
        [StringLength(10)]
        public string? Ticker { get; init; }
        [Required, StringLength(300)]
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public byte PriceSourceTypeId { get; init; }
        public byte ReportSourceTypeId { get; init; }
        public string? ReportSourceValue { get; init; }
    }
}
