using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class CompanySource
    {
        public string CompanyId { get; set; } = null!;
        public short SourceId { get; set; }
        public string? Value { get; set; }

        public virtual Company Company { get; set; } = null!;
        public virtual Source Source { get; set; } = null!;
    }
}
