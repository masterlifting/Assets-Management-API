using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Coefficient
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public decimal Pe { get; set; }
        public decimal Pb { get; set; }
        public decimal DebtLoad { get; set; }
        public decimal Profitability { get; set; }
        public decimal Roa { get; set; }
        public decimal Roe { get; set; }
        public decimal Eps { get; set; }
        public long ReportId { get; set; }

        public virtual Report Report { get; set; } = null!;
    }
}
