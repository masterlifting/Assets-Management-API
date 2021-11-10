using IM.Service.Common.Net.Attributes;
using IM.Service.Common.Net.Models.Dto.Mq.Companies;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Company.Models.Dto
{
    public record CompanyPutDto
    {
        [StringLength(100, MinimumLength = 4)]
        public string Name { get; init; } = null!;

        [NotZero(nameof(IndustryId))]
        public byte IndustryId { get; init; }

        public string? Description { get; init; }

        public IEnumerable<EntityTypeDto>? DataSources { get; init; }
        public IEnumerable<EntityTypeDto>? Brokers { get; init; }
    }
}
