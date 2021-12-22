using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class ExchangeRate
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateOperation { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public long Identifier { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public long TransactionStatusId { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
        public virtual TransactionStatus TransactionStatus { get; set; } = null!;
    }
}
