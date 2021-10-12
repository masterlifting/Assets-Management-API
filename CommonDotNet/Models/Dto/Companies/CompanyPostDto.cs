
using System.ComponentModel.DataAnnotations;


namespace CommonServices.Models.Dto.Companies
{
    public class CompanyPostDto : CompanyPutDto
    {
        [StringLength(10)] 
        public string Ticker { get; init; } = null!;
        public string Industry { get; init; } = null!;
        public string Sector { get; init; } = null!;
    }
}
