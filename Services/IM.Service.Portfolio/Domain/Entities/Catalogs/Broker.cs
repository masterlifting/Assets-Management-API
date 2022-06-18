using IM.Service.Shared.Models.Entity;
using IM.Service.Portfolio.Domain.Entities.ManyToMany;

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IM.Service.Portfolio.Domain.Entities.Catalogs;

public class Broker : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Deal>? Deals { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Event>? Events { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Account>? Accounts { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<BrokerExchange>? BrokerExchanges { get; set; }
    [JsonIgnore]
    public virtual IEnumerable<Report>? Reports { get; set; }
}