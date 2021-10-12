using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Lot
    {
        public Lot()
        {
            Tickers = new HashSet<Ticker>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public int Value { get; set; }

        public virtual ICollection<Ticker> Tickers { get; set; }
    }
}
