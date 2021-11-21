using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities.Companies
{
    public partial class StockSplit
    {
        public string CompanyTicker { get; set; } = null!;
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public int Divider { get; set; }

        public virtual Company CompanyTickerNavigation { get; set; } = null!;
    }
}
