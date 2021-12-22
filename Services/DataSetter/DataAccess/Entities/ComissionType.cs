using System;
using System.Collections.Generic;

namespace DataSetter.DataAccess.Entities
{
    public partial class ComissionType
    {
        public ComissionType()
        {
            Comissions = new HashSet<Comission>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Comission> Comissions { get; set; }
    }
}
