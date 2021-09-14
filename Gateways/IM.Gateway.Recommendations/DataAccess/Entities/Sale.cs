using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Recommendations.DataAccess.Entities
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public string TickerName { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        public int Lot { get; set; }
        public decimal Price { get; set; }
        public int Percent { get; set; }
    }
}
