using System;
using CommonServices.Models.Dto.Http;

namespace CommonServices.Models.Entity
{
    public class PriceIdentity : IFilterDate
    {
        public string TickerName { get; init; } = null!;
        public DateTime Date { get; set; }
    }
}
