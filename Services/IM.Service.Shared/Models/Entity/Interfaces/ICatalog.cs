namespace IM.Service.Shared.Models.Entity.Interfaces;

public interface ICatalog
{
    byte Id { get; init; }
    string Name { get; set; }
    string? Description { get; set; }
}