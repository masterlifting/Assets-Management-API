using System.Collections.Generic;

namespace IM.Service.Broker.Data.DataAccess.Entities;

public class Company : Common.Net.Models.Entity.Company
{
    public IEnumerable<Stock>? Stocks { get; set; }
}