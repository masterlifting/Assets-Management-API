using System;

namespace IM.Service.Recommendations.Models.Api.Http;

public record SaleDto
{
    public string Company { get; init; } = null!;
    public SaleRecommendationDto[] Recommendations { get; init; } = Array.Empty<SaleRecommendationDto>();
}

public record SaleRecommendationDto
{
    public SaleRecommendationDto(decimal plan, decimal? fact, decimal value, decimal price)
    {
        var _fact = "not computed";

        if (fact.HasValue)
        {
            var _f = decimal.Round(fact.Value, 1);
            _fact = $"{_f}%";
            if (_f > 0)
                _fact = '+' + _fact;
        }

        Fact = _fact;
        Plan = $"{decimal.Round(plan, 1)}%";
        Value = $"{value:0.####}";
        Price = $"{price:0.####}";
    }
    public string Plan { get; init; }
    public string Fact { get; init; }
    public string Value { get; init; }
    public string Price { get; init; }
}