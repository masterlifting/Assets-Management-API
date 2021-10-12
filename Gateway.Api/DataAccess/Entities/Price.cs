using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Price
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime BidDate { get; set; }
        public decimal Value { get; set; }
        public long CurrencyId { get; set; }
        public long TickerId { get; set; }

        public virtual Currency Currency { get; set; }
        public virtual Ticker Ticker { get; set; }
    }
}
