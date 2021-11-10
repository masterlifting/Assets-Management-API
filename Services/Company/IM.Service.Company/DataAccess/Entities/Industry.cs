using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Company.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Industry : CommonEntityType
    {
        public virtual IEnumerable<Company>? Companies { get; set; }

        public virtual Sector Sector { get; set; } = null!;
        [Range(1, byte.MaxValue)]
        public byte SectorId { get; set; }
    }
}
