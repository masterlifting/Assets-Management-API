using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class Industry
    {
        public Industry()
        {
            Companies = new HashSet<Company>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Company> Companies { get; set; }
    }
}
