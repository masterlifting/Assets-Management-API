using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Attributes;


namespace IM.Service.Company.Models.Dto;

public record CompanyPostDto : CompanyPutDto
{
    [Key, StringLength(10, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;
}