using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Broker : CommonEntityType
{
    public IEnumerable<Account>? Accounts { get; set; } = null!;
}