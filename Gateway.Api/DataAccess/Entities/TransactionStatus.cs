using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class TransactionStatus
    {
        public TransactionStatus()
        {
            AccountTransactions = new HashSet<AccountTransaction>();
            ExchangeRates = new HashSet<ExchangeRate>();
            StockTransactions = new HashSet<StockTransaction>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AccountTransaction> AccountTransactions { get; set; }
        public virtual ICollection<ExchangeRate> ExchangeRates { get; set; }
        public virtual ICollection<StockTransaction> StockTransactions { get; set; }
    }
}
