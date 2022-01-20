using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;
using IM.Service.Company.Data.DataAccess.Entities.ManyToMany;

namespace IM.Service.Company.Data.DataAccess.Entities;

public class Sources : CommonEntityType
{
    public virtual IEnumerable<CompanySource>? CompanySources { get; init; }
}