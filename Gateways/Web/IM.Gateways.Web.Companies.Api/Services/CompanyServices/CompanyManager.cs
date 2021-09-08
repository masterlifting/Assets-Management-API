using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Companies.Api.DataAccess.Entities;
using IM.Gateways.Web.Companies.Api.DataAccess.Repository;
using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Services.RabbitServices;

using System.Linq;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.CompanyServices
{
    public class CompanyManager
    {
        private readonly RepositorySet<Company> repository;
        private readonly RabbitCrudService rabbitCrudService;

        public CompanyManager(RepositorySet<Company> repository, RabbitCrudService rabbitCrudService)
        {
            this.repository = repository;
            this.rabbitCrudService = rabbitCrudService;
        }

        public async Task<ResponseModel<string>> CreateCompanyAsync(CompanyPostDto company)
        {
            var ctxCompany = new Company()
            {
                Ticker = company.Ticker,
                Name = company.Name,
                Description = company.Description
            };

            var (errors, _) = await repository.CreateAsync(ctxCompany, company.Name);

            if (errors.Any())
                return new() { Errors = errors };

            rabbitCrudService.CreateCompany(company);

            return new() { Data = $"'{company.Name}' created." };
        }
        public async Task<ResponseModel<string>> UpdateCompanyAsync(string ticker, CompanyPostDto company)
        {
            var ctxCompany = new Company()
            {
                Ticker = ticker,
                Name = company.Name,
                Description = company.Description
            };

            var (errors, _) = await repository.UpdateAsync(ctxCompany, company.Name);

            if (errors.Any())
                return new() { Errors = errors };

            rabbitCrudService.UpdateCompany(company);

            return new() { Data = $"'{company.Name}' updated." };
        }
        public async Task<ResponseModel<string>> DeleteCompanyAsync(string ticker)
        {
            var errors = await repository.DeleteAsync(ticker.ToUpperInvariant().Trim(), ticker);

            if (errors.Any())
                return new() { Errors = errors };

            rabbitCrudService.DeleteCompany(ticker);

            return new() { Data = $"'{ticker}' deleted." };
        }
    }
}
