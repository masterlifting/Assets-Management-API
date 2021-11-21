using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Reports
{
    public partial class Ticker
    {
        public Ticker()
        {
            Reports = new HashSet<Report>();
        }

        public string Name { get; set; } = null!;
        public short SourceTypeId { get; set; }
        public string? SourceValue { get; set; }

        public virtual SourceType SourceType { get; set; } = null!;
        public virtual ICollection<Report> Reports { get; set; }
    }
}
