using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class Sector
    {
        public Sector()
        {
            Industries = new HashSet<Industry>();
        }

        public short Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public virtual ICollection<Industry> Industries { get; set; }
    }
}
