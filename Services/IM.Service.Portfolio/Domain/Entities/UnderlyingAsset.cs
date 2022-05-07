using IM.Service.Portfolio.Domain.Entities.Catalogs;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IM.Service.Portfolio.Domain.Entities;

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