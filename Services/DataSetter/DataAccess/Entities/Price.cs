using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Price
    {
        public string CompanyId { get; set; } = null!;
        public DateOnly Date { get; set; }
        public string SourceType { get; set; } = null!;
        public decimal Value { get; set; }

        public virtual Company Company { get; set; } = null!;
    }
}
