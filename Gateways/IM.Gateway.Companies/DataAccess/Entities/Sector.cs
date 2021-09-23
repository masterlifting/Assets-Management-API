using System.Collections.Generic;
using CommonServices.Models.Entity;

namespace IM.Gateway.Companies.DataAccess.Entities
{
    public class Sector : CommonEntityType
    {
        public virtual IEnumerable<Company>? Companies { get; set; }
    }
}
