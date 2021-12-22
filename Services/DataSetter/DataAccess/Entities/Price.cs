using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Price
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime BidDate { get; set; }
        public decimal Value { get; set; }
        public long CurrencyId { get; set; }
        public long TickerId { get; set; }

        public virtual Currency Currency { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;
    }
}
