using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Portfolio.DataAccess.Entities.Catalogs;

public class Currency : CommonEntityType
{
    public virtual IEnumerable<Deal>? Deals { get; set; }
    public virtual IEnumerable<Event>? Events { get; set; }
}