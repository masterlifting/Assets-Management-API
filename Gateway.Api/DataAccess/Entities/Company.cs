using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
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
        public string Name { get; set; }
        public long IndustryId { get; set; }
        public long SectorId { get; set; }
        public DateTime? DateSplit { get; set; }

        public virtual Industry Industry { get; set; }
        public virtual Sector Sector { get; set; }
        public virtual BuyRecommendation BuyRecommendation { get; set; }
        public virtual Rating Rating { get; set; }
        public virtual ReportSource ReportSource { get; set; }
        public virtual SellRecommendation SellRecommendation { get; set; }
        public virtual ICollection<CompanySummary> CompanySummaries { get; set; }
        public virtual ICollection<DividendSummary> DividendSummaries { get; set; }
        public virtual ICollection<Isin> Isins { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<Ticker> Tickers { get; set; }
    }
}
