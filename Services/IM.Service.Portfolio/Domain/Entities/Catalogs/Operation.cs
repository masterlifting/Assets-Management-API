using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;
using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class Operation : Catalog
{
    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<EventType>? EventTypes { get; set; }
}