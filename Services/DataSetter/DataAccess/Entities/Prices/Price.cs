using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Prices
{
    public partial class Price
    {
        public string TickerName { get; set; } = null!;
        public DateTime Date { get; set; }
        public string SourceType { get; set; } = null!;
        public decimal Value { get; set; }

        public virtual Ticker TickerNameNavigation { get; set; } = null!;
    }
}
