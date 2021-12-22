using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class ExchangeRateSummary
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public decimal TotalSoldQuantity { get; set; }
        public decimal TotalSoldCost { get; set; }
        public decimal TotalPurchasedQuantity { get; set; }
        public decimal TotalPurchasedCost { get; set; }
        public decimal AvgSoldRate { get; set; }
        public decimal AvgPurchasedRate { get; set; }
        public DateTime DateUpdate { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
    }
}
