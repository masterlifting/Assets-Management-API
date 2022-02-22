using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.DataAccess.Entities.ManyToMany;

namespace IM.Service.Portfolio.DataAccess.Entities;

public class User
{
    [Key]
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;

    public virtual IEnumerable<Account>? Accounts { get; set; } = null!;

    public virtual IEnumerable<Deal>? Deals { get; set; } = null!;
    public virtual IEnumerable<Event>? Events { get; set; } = null!;
    public virtual IEnumerable<Report>? Reports { get; set; } = null!;

    public virtual IEnumerable<BrokerUser>? BrokerUsers { get; set; } = null!;
}