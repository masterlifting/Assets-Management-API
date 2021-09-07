using IM.Gateways.Web.Companies.Api.DataAccess.Entities;

namespace IM.Gateways.Web.Companies.Api.Models.Dto
{
    public class CompanyPostDto : Company
    {
        public byte PriceSourceTypeId { get; set; }
        public byte ReportSourceTypeId { get; set; }
        public string? ReportSourceValue { get; set; }
    }
}
