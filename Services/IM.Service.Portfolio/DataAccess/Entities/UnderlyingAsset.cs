using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using IM.Service.Portfolio.DataAccess.Entities.Catalogs;

namespace IM.Service.Portfolio.DataAccess.Entities;

public class UnderlyingAsset
{
    [Key]
    public string Id { get; init; } = null!;
    
    public virtual UnderlyingAssetType UnderlyingAssetType { get; set; } = null!;
    public byte UnderlyingAssetTypeId { get; set; }

    public virtual Country Country { get; set; } = null!;
    public byte CountryId { get; set; }

    public string Name { get; set; } = null!;

    public virtual IEnumerable<Derivative>? Derivatives { get; set; }
}