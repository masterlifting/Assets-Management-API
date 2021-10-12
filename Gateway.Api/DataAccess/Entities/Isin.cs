using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Isin
    {
        public Isin()
        {
            Dividends = new HashSet<Dividend>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; }
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; }
        public virtual ICollection<Dividend> Dividends { get; set; }
    }
}
