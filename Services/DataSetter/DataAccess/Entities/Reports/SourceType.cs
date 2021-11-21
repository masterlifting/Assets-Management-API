using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Reports
{
    public partial class SourceType
    {
        public SourceType()
        {
            Tickers = new HashSet<Ticker>();
        }

        public short Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Ticker> Tickers { get; set; }
    }
}
