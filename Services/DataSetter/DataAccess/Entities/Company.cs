using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Company
    {
        public Company()
        {
            CompanySummaries = new HashSet<CompanySummary>();
            DividendSummaries = new HashSet<DividendSummary>();
            Isins = new HashSet<Isin>();
            Reports = new HashSet<Report>();
            Tickers = new HashSet<Ticker>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; } = null!;
        public long IndustryId { get; set; }
        public long SectorId { get; set; }
        public DateTime? DateSplit { get; set; }

        public virtual Industry Industry { get; set; } = null!;
        public virtual Sector Sector { get; set; } = null!;
        public virtual BuyRecommendation BuyRecommendation { get; set; } = null!;
        public virtual Rating Rating { get; set; } = null!;
        public virtual ReportSource ReportSource { get; set; } = null!;
        public virtual SellRecommendation SellRecommendation { get; set; } = null!;
        public virtual ICollection<CompanySummary> CompanySummaries { get; set; }
        public virtual ICollection<DividendSummary> DividendSummaries { get; set; }
        public virtual ICollection<Isin> Isins { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Ticker> Tickers { get; set; }
    }
}
