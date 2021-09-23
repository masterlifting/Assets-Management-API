
using System.ComponentModel.DataAnnotations;


namespace CommonServices.Models.Dto.GatewayCompanies
{
    public class CompanyPostDto : CompanyPutDto
    {
        [StringLength(10)] public string Ticker { get; init; } = null!;
    }
}
