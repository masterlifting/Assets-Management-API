using System.Collections.Generic;
using IM.Service.Broker.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Company : Common.Net.Models.Entity.Company
{
    public IEnumerable<CompanyExchange>? CompanyExchanges { get; set; }
}