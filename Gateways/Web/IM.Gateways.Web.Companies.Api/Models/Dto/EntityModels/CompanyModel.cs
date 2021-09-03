using IM.Gateways.Web.Companies.Api.DataAccess.Entities;
using IM.Gateways.Web.Companies.Api.Services.Attributes;

namespace IM.Gateways.Web.Companies.Api.Models.Dto.State
{
    public class CompanyModel : Company
    {
        [Zero]
        public byte PriceSourceTypeId { get; set; }
        public byte ReportSourceTypeId { get; set; }
        public string? ReportSourceValue { get; set; }
    }
}
