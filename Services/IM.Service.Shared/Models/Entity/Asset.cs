using System.ComponentModel.DataAnnotations;

using IM.Service.Shared.Attributes;
using IM.Service.Shared.Models.Entity.Interfaces;

namespace IM.Service.Shared.Models.Entity;

public abstract class Asset<TAsset, TAssetType, TCountry> : IAsset
    where TAsset : class, IAsset
    where TAssetType : class, IAssetType<TAsset>
    where TCountry : class, ICountry<TAsset>
{
    [Required, StringLength(20, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;

    public virtual TAssetType Type { get; set; } = null!;
    public byte TypeId { get; init; }

    public virtual TCountry Country { get; set; } = null!;
    public byte CountryId { get; set; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = null!;
    
    [StringLength(500)]
    public string? Description { get; set; }
}