using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Identifier
{
    public int Id { get; set; }
    public string Value { get; set; } = null!;
    public string? Description { get; set; }

    public IEnumerable<Transaction>? Transactions { get; set; }
}