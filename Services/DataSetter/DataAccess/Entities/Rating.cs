using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Rating
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public int Place { get; set; }
        public decimal Result { get; set; }
        public decimal? PriceComparisonValue { get; set; }
        public decimal? ReportComparisonValue { get; set; }
        public decimal? CashFlowPositiveBalanceValue { get; set; }
        public decimal? CoefficientComparisonValue { get; set; }
        public decimal? CoefficientAverageValue { get; set; }
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; } = null!;
    }
}
