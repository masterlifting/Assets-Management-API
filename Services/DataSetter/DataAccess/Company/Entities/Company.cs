using System.ComponentModel.DataAnnotations;

namespace DataSetter.DataAccess.Company.Entities;

public class Company : IM.Service.Common.Net.Models.Entity.Company
{
    [Range(1, byte.MaxValue)]
    public byte IndustryId { get; set; }
    public virtual Industry Industry { get; set; } = null!;

    public string? Description { get; set; }
}