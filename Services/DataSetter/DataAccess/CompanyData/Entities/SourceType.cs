using IM.Service.Common.Net.Models.Entity;

using System.Collections.Generic;

namespace DataSetter.DataAccess.CompanyData.Entities;

public class SourceType : Catalog
{
    public virtual IEnumerable<CompanySourceType>? CompanySourceTypes { get; init; }
}