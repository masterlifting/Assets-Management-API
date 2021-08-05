using IM.Gateways.Web.Companies.Api.Models.Dto.State;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces
{
    public interface IRabbitmqManager
    {
        Task CreateCompanyAsync(CompanyModel company);
        Task UpdateCompanyAsync(CompanyModel company);
        Task DeleteCompanyAsync(string ticker);
    }
}
