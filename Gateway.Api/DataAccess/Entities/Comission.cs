using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Comission
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateOperation { get; set; }
        public long AccountId { get; set; }
        public long CurrencyId { get; set; }
        public decimal Amount { get; set; }
        public long ComissionTypeId { get; set; }

        public virtual Account Account { get; set; }
        public virtual ComissionType ComissionType { get; set; }
        public virtual Currency Currency { get; set; }
    }
}
