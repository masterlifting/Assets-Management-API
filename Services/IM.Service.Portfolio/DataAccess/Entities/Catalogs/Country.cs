using System.Collections.Generic;
using IM.Service.Common.Net.Models.Entity;

namespace IM.Service.Portfolio.DataAccess.Entities.Catalogs;

public class Country : Catalog
{
    public virtual IEnumerable<UnderlyingAsset>? UnderlyingAssets { get; set; }
}