using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Report
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public DateTime DateReport { get; set; }
        public bool IsChecked { get; set; }
        public long StockVolume { get; set; }
        public decimal Revenue { get; set; }
        public decimal NetProfit { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal CashFlow { get; set; }
        public decimal Assets { get; set; }
        public decimal Turnover { get; set; }
        public decimal ShareCapital { get; set; }
        public decimal Dividends { get; set; }
        public decimal Obligations { get; set; }
        public decimal LongTermDebt { get; set; }
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; }
        public virtual Coefficient Coefficient { get; set; }
    }
}
