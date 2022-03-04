﻿using System.ComponentModel.DataAnnotations;
using IM.Service.Common.Net.Attributes;

namespace IM.Service.Data.Models.Api.Http;

public record CompanyGetDto
{
    public string Id { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string Country { get; init; } = null!;
    public string Industry { get; init; } = null!;
    public string Sector { get; init; } = null!;
    public string? Description { get; init; }
    public SourceGetDto[]? Sources { get; set; }
}
public record CompanyPutDto
{
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = null!;

    [NotZero(nameof(IndustryId))]
    public byte IndustryId { get; init; }

    [NotZero(nameof(CountryId))]
    public byte CountryId { get; init; }

    public string? Description { get; init; }

    public IEnumerable<SourcePostDto>? Sources { get; init; }
}
public record CompanyPostDto : CompanyPutDto
{
    [StringLength(10, MinimumLength = 1), Upper]
    public string Id { get; init; } = null!;
}

public record SourceGetDto(string Name, string? Value);
public record SourcePostDto(byte Id, string? Value);