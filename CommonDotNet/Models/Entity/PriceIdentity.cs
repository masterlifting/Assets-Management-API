using System;

namespace CommonServices.Models.Entity
{
    public class PriceIdentity
    {
        public DateTime Date { get; set; }
        public string TickerName { get; set; } = null!;
    }
}
