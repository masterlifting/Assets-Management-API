using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Isin
    {
        public Isin()
        {
            Dividends = new HashSet<Dividend>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; } = null!;
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual ICollection<Dividend> Dividends { get; set; }
    }
}
