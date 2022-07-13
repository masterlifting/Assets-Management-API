using System.Collections.Generic;

namespace IM.Service.Shared.Models.Entity.Interfaces;

public interface ICountry<TAsset> : ICatalog where TAsset : class, IAsset
{
    IEnumerable<TAsset>? Assets { get; set; }
}