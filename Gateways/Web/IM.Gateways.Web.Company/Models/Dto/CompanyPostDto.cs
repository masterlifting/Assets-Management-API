
using System.ComponentModel.DataAnnotations;

namespace IM.Gateways.Web.Company.Models.Dto
{
    public class CompanyPostDto
    {
        [StringLength(10)]
        public string? Ticker { get; set; } = null!;
        [Required, StringLength(300)]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public byte PriceSourceTypeId { get; set; }
        public byte ReportSourceTypeId { get; set; }
        public string? ReportSourceValue { get; set; }
    }
}
