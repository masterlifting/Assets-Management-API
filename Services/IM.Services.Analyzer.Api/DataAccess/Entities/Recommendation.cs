using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Services.Analyzer.Api.DataAccess.Entities
{
    public class Recommendation
    {
        [Key]
        public int Id { get; set; }

        public string TickerName { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        public virtual ToBuy Buy { get; set; } = null!;
        public virtual List<ToSell> Sells { get; set; } = null!;
    }

    public class ToBuy
    {
        [Key]
        public int Id { get; set; }

        public decimal Price { get; set; }
        public int Percent { get; set; }

        public virtual Recommendation Recommendation { get; set; } = null!;
        public int RecommendationId { get; set; }
    }
    public class ToSell
    {
        [Key]
        public int Id { get; set; }

        public int Lot { get; set; }
        public decimal Price { get; set; }
        public int Percent { get; set; }

        public virtual Recommendation Recommendation { get; set; } = null!;
        public int RecommendationId { get; set; }
    }
}
