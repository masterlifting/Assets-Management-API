using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Attributes;

namespace IM.Service.Market.Models.Api.Http;

public record CompanyGetDto
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string Industry { get; init; } = null!;
    public string Sector { get; init; } = null!;
    public string? Description { get; init; }
}
public record CompanyPostDto : CompanyPutDto
{
    [StringLength(10, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;
}

public record CompanyPutDto
{
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = null!;
    [MoreZero(nameof(IndustryId))]
    public byte IndustryId { get; init; }
    [MoreZero(nameof(CountryId))]
    public byte CountryId { get; init; }
    public string? Description { get; init; }
}

