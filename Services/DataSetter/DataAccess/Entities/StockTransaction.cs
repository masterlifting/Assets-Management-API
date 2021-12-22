using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class StockTransaction
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateOperation { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        public decimal Cost { get; set; }
        public long TickerId { get; set; }
        public long TransactionStatusId { get; set; }
        public long ExchangeId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual Exchange Exchange { get; set; } = null!;
        public virtual Ticker Ticker { get; set; } = null!;
        public virtual TransactionStatus TransactionStatus { get; set; } = null!;
    }
}
