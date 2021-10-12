// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace CommonServices.Models.Dto
{
    public record CompanyFullDto
    {
        public string Name { get; init; } = null!;
        public string Ticker { get; init; } = null!;
        public string Industry { get; init; } = null!;
        public string Sector { get; init; } = null!;
        public string? PriceSource { get; init; }
        public string? ReportSource { get; init; }
        public string? ReportSourceValue { get; init; }
        public string? Description { get; init; }
    }
}
