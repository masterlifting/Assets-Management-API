using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Company.DataAccess.Entities;
using System.Linq;
using System.Threading.Tasks;
using IM.Gateways.Web.Company.DataAccess.Repository;
using IM.Gateways.Web.Company.Models.Dto;
using IM.Gateways.Web.Company.Services.RabbitServices;

namespace IM.Gateways.Web.Company.Services.CompanyServices
{
    public class CompanyManager
    {
        private readonly RepositorySet<DataAccess.Entities.Company> repository;
        private readonly RabbitCrudService rabbitCrudService;

        public CompanyManager(RepositorySet<DataAccess.Entities.Company> repository, RabbitCrudService rabbitCrudService)
        {
            this.repository = repository;
            this.rabbitCrudService = rabbitCrudService;
        }

        public async Task<ResponseModel<string>> CreateCompanyAsync(CompanyPostDto company)
        {
            var ctxCompany = new DataAccess.Entities.Company()
            {
                Ticker = company.Ticker!,
                Name = company.Name,
                Description = company.Description
            };

            var (errors, createdCompany) = await repository.CreateAsync(ctxCompany, company.Name);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.CreateCompany(new ()
            {
                Name = createdCompany!.Name,
                Ticker = createdCompany.Ticker,
                Description = createdCompany.Description,
                PriceSourceTypeId = company.PriceSourceTypeId,
                ReportSourceTypeId = company.ReportSourceTypeId,
                ReportSourceValue = company.ReportSourceValue
            });

            return new ResponseModel<string> { Data = $"'{company.Name}' created" };
        }
        public async Task<ResponseModel<string>> UpdateCompanyAsync(string ticker, CompanyPostDto company)
        {
            var ctxCompany = new DataAccess.Entities.Company()
            {
                Ticker = ticker,
                Name = company.Name,
                Description = company.Description
            };

            var (errors, updatedCompany) = await repository.UpdateAsync(ctxCompany, ticker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.UpdateCompany(new()
            {
                Name = updatedCompany!.Name,
                Ticker = updatedCompany.Ticker,
                Description = updatedCompany.Description,
                PriceSourceTypeId = company.PriceSourceTypeId,
                ReportSourceTypeId = company.ReportSourceTypeId,
                ReportSourceValue = company.ReportSourceValue
            });

            return new ResponseModel<string> { Data = $"'{company.Name}' updated" };
        }
        public async Task<ResponseModel<string>> DeleteCompanyAsync(string ticker)
        {
            var errors = await repository.DeleteAsync(ticker.ToUpperInvariant().Trim(), ticker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.DeleteCompany(ticker);

            return new ResponseModel<string> { Data = $"'{ticker}' deleted" };
        }
    }
}
