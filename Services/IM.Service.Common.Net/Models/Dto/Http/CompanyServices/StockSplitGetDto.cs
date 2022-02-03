using System;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public record StockSplitGetDto
{
    public string Company { get; init; } = null!;
    public DateOnly Date { get; init; }
    public string SourceType { get; init; } = null!;
    public int Value { get; init; }
}