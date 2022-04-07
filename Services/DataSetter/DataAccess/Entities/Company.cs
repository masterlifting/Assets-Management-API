using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Company
    {
        public Company()
        {
            CompanySources = new HashSet<CompanySource>();
            Prices = new HashSet<Price>();
            Reports = new HashSet<Report>();
            StockSplits = new HashSet<StockSplit>();
            StockVolumes = new HashSet<StockVolume>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public short IndustryId { get; set; }

        public virtual Industry Industry { get; set; } = null!;
        public virtual ICollection<CompanySource> CompanySources { get; set; }
        public virtual ICollection<Price> Prices { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<StockSplit> StockSplits { get; set; }
        public virtual ICollection<StockVolume> StockVolumes { get; set; }
    }
}
