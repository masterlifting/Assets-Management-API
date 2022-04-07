using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Report
    {
        public string CompanyId { get; set; } = null!;
        public int Year { get; set; }
        public short Quarter { get; set; }
        public string SourceType { get; set; } = null!;
        public int Multiplier { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? ProfitNet { get; set; }
        public decimal? ProfitGross { get; set; }
        public decimal? CashFlow { get; set; }
        public decimal? Asset { get; set; }
        public decimal? Turnover { get; set; }
        public decimal? ShareCapital { get; set; }
        public decimal? Obligation { get; set; }
        public decimal? LongTermDebt { get; set; }

        public virtual Company Company { get; set; } = null!;
    }
}
