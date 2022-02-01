using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Exchange : CommonEntityType
{
    public IEnumerable<Stock>? Stocks { get; set; }
}