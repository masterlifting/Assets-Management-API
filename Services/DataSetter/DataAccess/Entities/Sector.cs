using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Sector
    {
        public Sector()
        {
            Companies = new HashSet<Company>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Company> Companies { get; set; }
    }
}
