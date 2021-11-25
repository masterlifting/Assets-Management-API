using System.ComponentModel.DataAnnotations;

namespace IM.Service.Company.DataAccess.Entities;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Company : Common.Net.Models.Entity.Companies.Company
{
    [Range(1, byte.MaxValue)]
    public byte IndustryId { get; set; }
    public virtual Industry Industry { get; set; } = null!;

    public string? Description { get; set; }
}