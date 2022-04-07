using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Source
    {
        public Source()
        {
            CompanySources = new HashSet<CompanySource>();
        }

        public short Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<CompanySource> CompanySources { get; set; }
    }
}
