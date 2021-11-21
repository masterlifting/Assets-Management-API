using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Reports
{
    public partial class Report
    {
        public string TickerName { get; set; } = null!;
        public int Year { get; set; }
        public byte Quarter { get; set; }
        public string SourceType { get; set; } = null!;
        public int Multiplier { get; set; }
        public long StockVolume { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? ProfitNet { get; set; }
        public decimal? ProfitGross { get; set; }
        public decimal? CashFlow { get; set; }
        public decimal? Asset { get; set; }
        public decimal? Turnover { get; set; }
        public decimal? ShareCapital { get; set; }
        public decimal? Dividend { get; set; }
        public decimal? Obligation { get; set; }
        public decimal? LongTermDebt { get; set; }

        public virtual Ticker TickerNameNavigation { get; set; } = null!;
    }
}
