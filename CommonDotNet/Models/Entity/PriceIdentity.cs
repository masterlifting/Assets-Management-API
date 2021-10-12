
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommonServices.Models.Entity
{
    public class PriceIdentity
    {
        public string TickerName { get; init; } = null!;
        [Column(TypeName = "Date")]
        public DateTime Date { get; init; }
    }
}
