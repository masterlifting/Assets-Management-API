using System;

namespace IM.Service.Recommendations.Models.Api.Http;

public record PurchaseDto
{
    public string Company { get; init; } = null!;
    public PurchaseRecommendationDto[] Recommendations { get; init; } = Array.Empty<PurchaseRecommendationDto>();
}

public record PurchaseRecommendationDto
{
    public string Plan { get; init; } = null!;
    public string? Fact { get; init; }
    public decimal Price { get; init; }
    public bool IrReady { get; init; }
}