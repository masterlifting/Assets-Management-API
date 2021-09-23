using CommonServices.HttpServices;
using CommonServices.Models.Http;

using IM.Gateway.Companies.DataAccess.Entities;
using IM.Gateway.Companies.DataAccess.Repository;
using CommonServices.Models.Dto.GatewayCompanies;
using IM.Gateway.Companies.Services.RabbitServices;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class DtoCompanyManager
    {
        private readonly RepositorySet<Company> repository;
        private readonly RabbitCrudService rabbitCrudService;

        public DtoCompanyManager(RepositorySet<Company> repository, RabbitCrudService rabbitCrudService)
        {
            this.repository = repository;
            this.rabbitCrudService = rabbitCrudService;
        }

        public async Task<ResponseModel<PaginatedModel<CompanyGetDto>>> GetAsync(HttpPagination pagination)
        {
            var errors = Array.Empty<string>();

            var count = await repository.GetCountAsync();
            var paginatedResult = repository.GetPaginationQuery(pagination, x => x.Name);

            var companies = await paginatedResult.Select(x => new CompanyGetDto
            {
                Name = x.Name,
                Ticker = x.Ticker,
                Sector = x.Sector.Name,
                Industry = x.Industry.Name,
                Description = x.Description
            })
            .ToArrayAsync();

            return new()
            {
                Errors = errors,
                Data = new()
                {
                    Items = companies,
                    Count = count
                }
            };
        }
        public async Task<ResponseModel<CompanyGetDto>> GetAsync(string ticker)
        {
            var company = await repository.FindAsync(ticker.ToUpperInvariant().Trim());

            if (company is null)
                return new() { Errors = new[] { "company not found" } };

            var sector = await repository.GetDbSetBy<Sector>().FindAsync(company.IndustryId);
            var industry = await repository.GetDbSetBy<Industry>().FindAsync(company.IndustryId);

            return new()
            {
                Data = new()
                {
                    Ticker = company.Ticker,
                    Name = company.Name,
                    Description = company.Description,
                    Sector = sector.Name,
                    Industry = industry.Name
                }
            };
        }
        public async Task<ResponseModel<string>> CreateAsync(CompanyPostDto model)
        {
            var ctxCompany = new Company()
            {
                Ticker = model.Ticker,
                Name = model.Name,
                SectorId = model.SectorId,
                IndustryId = model.IndustryId,
                Description = model.Description
            };

            var (errors, createdCompany) = await repository.CreateAsync(ctxCompany, model.Ticker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.CreateCompany(new()
            {
                Ticker = createdCompany!.Ticker,
                PriceSourceTypeId = model.PriceSourceTypeId,
                ReportSourceTypeId = model.ReportSourceTypeId,
                ReportSourceValue = model.ReportSourceValue
            });

            return new ResponseModel<string> { Data = $"'{createdCompany.Name}' created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(string ticker, CompanyPutDto model)
        {
            var ctxCompany = new Company()
            {
                Ticker = ticker,
                Name = model.Name,
                SectorId = model.SectorId,
                IndustryId = model.IndustryId,
                Description = model.Description
            };

            var (errors, updatedCompany) = await repository.UpdateAsync(ctxCompany, ticker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.UpdateCompany(new()
            {
                Ticker = updatedCompany!.Ticker,
                PriceSourceTypeId = model.PriceSourceTypeId,
                ReportSourceTypeId = model.ReportSourceTypeId,
                ReportSourceValue = model.ReportSourceValue
            });

            return new ResponseModel<string> { Data = $"'{updatedCompany.Name}' updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(string ticker)
        {
            var checkedTicker = ticker.ToUpperInvariant().Trim();

            var errors = await repository.DeleteAsync(checkedTicker, checkedTicker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.DeleteCompany(checkedTicker);

            return new ResponseModel<string> { Data = $"'{checkedTicker}' deleted" };
        }
    }
}
