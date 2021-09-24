using System;
using System.ComponentModel.DataAnnotations;

namespace IM.Gateway.Recommendations.DataAccess.Entities
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        public string TickerName { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

        public decimal Price { get; set; }
        public int Percent { get; set; }
    }
}