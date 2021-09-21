namespace IM.Gateway.Companies.Models.Dto
{
    public class CompanyGetDto
    {
        public string Ticker { get; init; } = null!;
        public string Name { get; init; } = null!;
        public string? Description { get; init; } = null!;
    }
}
