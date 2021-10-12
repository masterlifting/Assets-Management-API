using System;
using System.Collections.Generic;

#nullable disable

namespace Gateway.Api.DataAccess.Entities
{
    public partial class ComissionType
    {
        public ComissionType()
        {
            Comissions = new HashSet<Comission>();
        }

        public long Id { get; set; }
        public DateTime DateUpdate { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Comission> Comissions { get; set; }
    }
}
