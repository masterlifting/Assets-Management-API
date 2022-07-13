using System;

namespace IM.Service.Recommendations.Models.Api.Http;

public record SaleDto
{
    public string Asset { get; init; } = null!;
    public SaleRecommendationDto[] Recommendations { get; init; } = Array.Empty<SaleRecommendationDto>();
}

public record SaleRecommendationDto
{
    public SaleRecommendationDto(decimal profitPlan, decimal? profitFact, decimal activevalue, decimal costPlan, decimal? costFact)
    {
        var _profitFact = "not computed";

        if (profitFact.HasValue)
        {
            var _pf = decimal.Round(profitFact.Value, 1);
            _profitFact = $"{_pf}%";
            if (_pf > 0)
                _profitFact = '+' + _profitFact;
        }

        ProfitFact = _profitFact;
        ProfitPlan = $"{decimal.Round(profitPlan, 1)}%";
        ActiveValue = $"{activevalue:0.##########}";
        CostPlan = $"{costPlan:0.##########}";
        CostFact = $"{costFact:0.##########}";
    }
    public string ProfitPlan { get; init; }
    public string ProfitFact { get; init; }
    public string ActiveValue { get; init; }
    public string CostPlan { get; init; }
    public string CostFact { get; init; }
}