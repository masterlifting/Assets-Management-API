using CommonServices.Attributes;

using System.ComponentModel.DataAnnotations;

namespace CommonServices.Models.Dto.Companies
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

        //[NotZero(nameof(IndustryId))]
        public byte IndustryId { get; init; }

        public string? Description { get; init; }
    }
}
