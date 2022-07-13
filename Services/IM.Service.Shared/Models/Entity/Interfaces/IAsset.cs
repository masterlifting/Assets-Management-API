namespace IM.Service.Shared.Models.Entity.Interfaces;

public interface IAsset
{
    string Id { get; init; }
    byte TypeId { get; init; }
    byte CountryId { get; set; }
    string Name { get; set; }
}