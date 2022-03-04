using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;
using IM.Service.Market.DataAccess.Entities.ManyToMany;

namespace IM.Service.Market.DataAccess.Entities;

public class Sources : Catalog
{
    public virtual IEnumerable<CompanySource>? CompanySources { get; init; }
}