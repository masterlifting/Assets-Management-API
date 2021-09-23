using CommonServices.Attributes;

using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class CompanyPutDto
    {
        [Required, StringLength(300)]
        public string Name { get; init; } = null!;

        [NotZero(nameof(PriceSourceTypeId))]
        public byte PriceSourceTypeId { get; init; }

        [NotZero(nameof(ReportSourceTypeId))]
        public byte ReportSourceTypeId { get; init; }
        public string? ReportSourceValue { get; init; }

        [NotZero(nameof(IndustryId))]
        public byte IndustryId { get; init; }

        [NotZero(nameof(SectorId))]
        public byte SectorId { get; init; }

        public string? Description { get; init; }
    }
}
