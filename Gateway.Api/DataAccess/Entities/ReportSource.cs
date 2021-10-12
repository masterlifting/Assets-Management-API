using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class ReportSource
    {
        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public long CompanyId { get; set; }

        public virtual Company Company { get; set; }
    }
}
