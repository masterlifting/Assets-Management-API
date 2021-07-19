using IM.Gateways.Web.Companies.Api.DataAccess.Entities;

namespace IM.Gateways.Web.Companies.Api.Models.Dto.State
{
    public class CompanyModel : Company
    {
        public byte? PriceSourceTypeId { get; set; }
        public ReportSourceModel? ReportSource { get; set; }
    }
}
