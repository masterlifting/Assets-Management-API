using IM.Gateways.Web.Companies.Api.Models.Dto.State;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces
{
    public interface ICrudExchange
    {
        void CreateCompany(CompanyModel company);
        void UpdateCompany(CompanyModel company);
        void DeleteCompany(string ticker);
    }
}
