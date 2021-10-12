using System.Collections.Generic;
using CommonServices.Models.Entity;

namespace IM.Service.Companies.DataAccess.Entities
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Sector : CommonEntityType
    {
        public virtual IEnumerable<Industry>? Industries { get; set; }
    }
}
