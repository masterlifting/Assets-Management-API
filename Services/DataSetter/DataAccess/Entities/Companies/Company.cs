using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Companies
{
    public partial class Company
    {
        public Company()
        {
            StockSplits = new HashSet<StockSplit>();
        }

        public string Ticker { get; set; } = null!;
        public string Name { get; set; } = null!;
        public short IndustryId { get; set; }
        public string? Description { get; set; }

        public virtual Industry Industry { get; set; } = null!;
        public virtual ICollection<StockSplit> StockSplits { get; set; }
    }
}
