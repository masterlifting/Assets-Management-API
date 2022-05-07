using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;
using IM.Service.Portfolio.Domain.Entities;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class EventType : Catalog
{
    public virtual IEnumerable<Event>? Events { get; set; }
    
    public virtual Operation Operation { get; set; } = null!;
    public byte OperationId { get; init; }
}