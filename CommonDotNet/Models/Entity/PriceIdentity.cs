using System;

namespace CommonServices.Models.Entity
{
    public class PriceIdentity
    {
        public string TickerName { get; init; } = null!;
        public DateTime Date { get; init; }
    }
}
