using System;
using System.Collections.Generic;

namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class RecommendationDto
    {
        public string Ticker { get; set; } = null!;
        public DateTime UpdateTime { get; set; }

        public ToBuyDto? Buy { get; set; }
        public List<ToSellDto>? Sells { get; set; }
    }
    public class ToBuyDto
    {
        public decimal Price { get; }
        public int Percent { get; }
    }
    public class ToSellDto
    {
        public int Lot { get; }
        public decimal Price { get; }
        public int Percent { get; }
    }
}
