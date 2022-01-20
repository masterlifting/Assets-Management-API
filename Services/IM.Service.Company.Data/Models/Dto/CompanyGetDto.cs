using IM.Service.Common.Net.Models.Dto;

namespace IM.Service.Company.Data.Models.Dto;

public record CompanyGetDto
{
    public string Ticker { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Industry { get; init; } = null!;
    public string Sector { get; init; } = null!;
    public string? Description { get; init; }
    public EntityTypeGetDto[]? Sources { get; set; }
}