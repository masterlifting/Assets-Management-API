using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class DividendSummary
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public decimal TotalSum { get; set; }
        public decimal TotalTax { get; set; }
        public DateTime DateUpdate { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Company Company { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
    }
}
