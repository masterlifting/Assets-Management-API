using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class AccountSummary
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public decimal FreeSum { get; set; }
        public decimal InvestedSum { get; set; }
        public DateTime DateUpdate { get; set; }

        public virtual Account Account { get; set; } = null!;
        public virtual Currency Currency { get; set; } = null!;
    }
}
