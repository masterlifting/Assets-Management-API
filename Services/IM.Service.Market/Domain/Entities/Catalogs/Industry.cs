using IM.Service.Shared.Models.Entity;

using System.Text.Json.Serialization;

namespace IM.Service.Market.Domain.Entities.Catalogs;

public class Industry : Catalog
{
    [JsonIgnore]
    public virtual IEnumerable<Company>? Companies { get; set; }

    public virtual Sector Sector { get; set; } = null!;
    public byte SectorId { get; set; }
}