using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Portfolio.DataAccess.Entities.Catalogs;

public class Operation : CommonEntityType
{
    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<EventType>? EventTypes { get; set; }
}