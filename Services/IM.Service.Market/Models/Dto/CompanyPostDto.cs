using IM.Service.Common.Net.Attributes;

using System.ComponentModel.DataAnnotations;


namespace IM.Service.Market.Models.Dto;

public record CompanyPostDto : CompanyPutDto
{
    [Key, StringLength(10, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;
}