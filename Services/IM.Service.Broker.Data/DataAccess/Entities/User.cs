using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class User
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;

    public IEnumerable<Account>? Accounts { get; set; } = null!;
}