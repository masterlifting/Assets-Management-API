using System;

namespace CommonServices.Models.Entity
{
    public class PriceIdentity
    {
        public string TickerName { get; set; } = null!;
        public DateTime Date { get; set; }
    }
}
