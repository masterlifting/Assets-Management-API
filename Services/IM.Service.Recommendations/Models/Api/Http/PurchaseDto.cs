using System;

namespace IM.Service.Recommendations.Models.Api.Http;

public record PurchaseDto
{
    public string Asset { get; init; } = null!;
    public PurchaseRecommendationDto[] Recommendations { get; init; } = Array.Empty<PurchaseRecommendationDto>();
}

public record PurchaseRecommendationDto
{
    public PurchaseRecommendationDto(decimal discountPlan, decimal? discountFact, decimal costPlan, decimal costFact, decimal? costNext)
    {
        var _discountFact = "not computed";

        if (discountFact.HasValue)
        {
            var _df = decimal.Round(discountFact.Value, 1);
            _discountFact = $"{_df}%";
            if (_df > 0)
                _discountFact = '+' + _discountFact;
        }

        DiscountPlan = $"{decimal.Round(discountPlan, 1)}%";
        DiscountFact = _discountFact;
        CostPlan = $"{costPlan:0.##########}";
        CostFact = $"{costFact:0.##########}";
        CostNext = costNext.HasValue ? $"{costNext.Value:0.##########}" : "not found";
    }
    public string DiscountPlan { get; init; }
    public string DiscountFact { get; init; }
    public string CostPlan { get; init; }
    public string CostFact { get; init; }
    public string CostNext { get; init; }
}