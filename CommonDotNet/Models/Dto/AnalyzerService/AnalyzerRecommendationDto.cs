using System;
using System.Collections.Generic;

namespace CommonServices.Models.Dto.AnalyzerService
{
    public class AnalyzerRecommendationDto
    {
        public string Ticker { get; set; } = null!;
        public DateTime UpdateTime { get; set; }

        public ToBuyDto? Buy { get; set; }
        public List<ToSellDto>? Sells { get; set; }
    }
    public class ToBuyDto
    {
        public decimal Price { get; set; }
        public int Percent { get; set; }
    }
    public class ToSellDto
    {
        public int Lot { get; set; }
        public decimal Price { get; set; }
        public int Percent { get; set; }
    }
}
