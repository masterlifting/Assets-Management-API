using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Industry
    {
        public Industry()
        {
            Companies = new HashSet<Company>();
        }

        public short Id { get; set; }
        public short SectorId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public virtual Sector Sector { get; set; } = null!;
        public virtual ICollection<Company> Companies { get; set; }
    }
}
