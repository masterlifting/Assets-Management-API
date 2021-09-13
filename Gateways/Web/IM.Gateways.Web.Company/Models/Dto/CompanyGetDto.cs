namespace IM.Gateways.Web.Company.Models.Dto
{
    public class CompanyGetDto
    {
        public string Ticker { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null!;
    }
}
