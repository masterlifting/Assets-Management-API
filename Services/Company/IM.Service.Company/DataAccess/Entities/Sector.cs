using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Company.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Sector : CommonEntityType
    {
        public virtual IEnumerable<Industry>? Industries { get; set; }
    }
}
