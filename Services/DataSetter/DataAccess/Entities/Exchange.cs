using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Exchange
    {
        public Exchange()
        {
            StockTransactions = new HashSet<StockTransaction>();
            Tickers = new HashSet<Ticker>();
            Weekends = new HashSet<Weekend>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<StockTransaction> StockTransactions { get; set; }
        public virtual ICollection<Ticker> Tickers { get; set; }
        public virtual ICollection<Weekend> Weekends { get; set; }
    }
}
