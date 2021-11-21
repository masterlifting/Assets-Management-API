using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Prices
{
    public partial class Ticker
    {
        public Ticker()
        {
            Prices = new HashSet<Price>();
        }

        public string Name { get; set; } = null!;
        public short SourceTypeId { get; set; }
        public string? SourceValue { get; set; }

        public virtual SourceType SourceType { get; set; } = null!;
        public virtual ICollection<Price> Prices { get; set; }
    }
}
