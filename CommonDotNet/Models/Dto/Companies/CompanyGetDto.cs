namespace CommonServices.Models.Dto.Companies
{
    public class CompanyGetDto
    {
        public string Name { get; init; } = null!;
        public string Ticker { get; init; } = null!;
        public string Industry { get; init; } = null!;
        public string Sector { get; init; } = null!;
        public string? Description { get; init; }
    }
}
