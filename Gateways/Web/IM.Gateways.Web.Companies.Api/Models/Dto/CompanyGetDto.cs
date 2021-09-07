namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class CompanyGetDto
    {
        public string Ticker { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
    }
}
