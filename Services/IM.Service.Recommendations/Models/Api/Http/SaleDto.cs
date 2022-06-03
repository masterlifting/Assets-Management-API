using System;

namespace IM.Service.Recommendations.Models.Api.Http;

public record SaleDto
{
    public string Company { get; init; } = null!;
    public SaleRecommendationDto[] Recommendations { get; init; } = Array.Empty<SaleRecommendationDto>();
}

public record SaleRecommendationDto
{
    public string Plan { get; init; } = null!;
    public string? Fact { get; init; }
    public decimal Value { get; init; }
    public decimal Price { get; init; }
    public bool IrReady { get; init; }
}