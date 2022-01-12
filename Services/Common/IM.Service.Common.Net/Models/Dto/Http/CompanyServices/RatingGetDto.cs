using System;

namespace IM.Service.Common.Net.Models.Dto.Http.CompanyServices;

public record RatingGetDto
{
    public string Company { get; init; } = null!;
    public int Place { get; init; }

    public decimal? Result { get; init; }
    public decimal? ResultPrice { get; init; }
    public decimal? ResultReport { get; init; }
    public decimal? ResultCoefficient { get; init; }
    
    public DateTime UpdateTime { get; init; }
}