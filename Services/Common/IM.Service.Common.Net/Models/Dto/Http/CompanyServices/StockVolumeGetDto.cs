using System;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public record StockVolumeGetDto
{
    public string Company { get; init; } = null!;
    public DateTime Date { get; init; }
    public string SourceType { get; init; } = null!;
    public long Value { get; init; }
}