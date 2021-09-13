using IM.Services.Recommendations.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IM.Services.Recommendations.Models.Dto
{
    public class RecommendationDto
    {
        public RecommendationDto() { }
        public RecommendationDto(Recommendation recommendation)
        {
            if (recommendation is null)
                throw new NullReferenceException($"{nameof(recommendation)} is null");

            Ticker = recommendation.TickerName;
            UpdateTime = recommendation.UpdateTime;

            Buy = recommendation.Buy is null ? null : new(recommendation.Buy.Price, recommendation.Buy.Percent);
            Sells = recommendation.Sells is null ? null : new(recommendation.Sells.Select(x => new ToSellDto(x.Lot, x.Price, x.Percent)));

        }
        public string Ticker { get; set; } = null!;
        public DateTime UpdateTime { get; set; }

        public ToBuyDto? Buy { get; set; }
        public List<ToSellDto>? Sells { get; set; }
    }

    public class ToBuyDto
    {
        public ToBuyDto() { }
        public ToBuyDto(decimal price, int percent)
        {
            Price = price;
            Percent = percent;
        }
        public decimal Price { get; }
        public int Percent { get; }
    }
    public class ToSellDto
    {
        public ToSellDto() { }
        public ToSellDto(int lot, decimal price, int percent)
        {
            Lot = lot;
            Price = price;
            Percent = percent;
        }
        public int Lot { get; }
        public decimal Price { get; }
        public int Percent { get; }
    }
}
