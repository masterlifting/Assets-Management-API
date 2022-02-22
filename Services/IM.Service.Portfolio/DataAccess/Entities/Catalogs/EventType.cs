using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Portfolio.DataAccess.Entities.Catalogs;

public class EventType : CommonEntityType
{
    public virtual IEnumerable<Event>? Events { get; set; }
    
    public virtual Operation Operation { get; set; } = null!;
    public byte OperationId { get; init; }
}