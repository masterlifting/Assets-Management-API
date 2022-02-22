using IM.Service.Common.Net.Attributes;
using IM.Service.Common.Net.Models.Dto;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Market.Models.Dto;

public record CompanyPutDto
{
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = null!;

    [NotZero(nameof(IndustryId))]
    public byte IndustryId { get; init; }

    public string? Description { get; init; }

    public IEnumerable<EntityTypePostDto>? Sources { get; init; }
}