using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.Domain.Entities.ManyToMany;

namespace IM.Service.Portfolio.Domain.Entities;

public class User
{
    [Key]
    public string Id { get; init; } = null!;
    public string Name { get; set; } = null!;

    public virtual IEnumerable<Account>? Accounts { get; set; } = null!;
    public virtual IEnumerable<BrokerUser>? BrokerUsers { get; set; } = null!;
}