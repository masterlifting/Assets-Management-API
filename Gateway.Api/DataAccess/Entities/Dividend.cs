using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Dividend
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateOperation { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public decimal Tax { get; set; }
        public long IsinId { get; set; }

        public virtual Account Account { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual Isin Isin { get; set; }
    }
}
