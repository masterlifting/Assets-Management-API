using IM.Gateways.Web.Companies.Api.Models.Dto.State;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces
{
    public interface IRabbitmqManager
    {
        bool CreateCompany(CompanyModel company);
        bool EditCompany(CompanyModel company);
        bool DeleteCompany(string ticker);
    }
}
