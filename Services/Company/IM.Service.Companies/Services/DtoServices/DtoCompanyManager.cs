using CommonServices.HttpServices;
using CommonServices.Models.Http;
using CommonServices.Models.Dto.Companies;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Service.Companies.DataAccess;
using IM.Service.Companies.DataAccess.Entities;
using IM.Service.Companies.DataAccess.Repository;
using IM.Service.Companies.Services.RabbitServices;

namespace IM.Service.Companies.Services.DtoServices
{
    public class DtoCompanyManager
    {
        private readonly DatabaseContext context;
        private readonly RepositorySet<Company> repository;
        private readonly RabbitCrudService rabbitCrudService;

        public DtoCompanyManager(DatabaseContext context, RepositorySet<Company> repository, RabbitCrudService rabbitCrudService)
        {
            this.context = context;
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
                Sector = x.Industry.Sector.Name,
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

            var industry = await repository.GetDbSetBy<Industry>().FindAsync(company.IndustryId);
            var sector = await repository.GetDbSetBy<Sector>().FindAsync(industry.SectorId);

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
            var sector = context.Sectors.FirstOrDefault(x => x.Name.Equals(model.Sector));

            if (sector is null)
            {
                sector = new Sector { Name = model.Sector };
                await context.Sectors.AddAsync(sector);
                await context.SaveChangesAsync();
            }

            var industry = context.Industries.FirstOrDefault(x => x.Name.Equals(model.Industry));

            if (industry is null)
            {
                industry = new Industry { SectorId = sector.Id, Name = model.Industry };
                await context.Industries.AddAsync(industry);
                await context.SaveChangesAsync();
            }

            var ctxCompany = new Company
            {
                Ticker = model.Ticker,
                Name = model.Name,
                IndustryId = industry.Id,
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
            var ctxCompany = new Company
            {
                Ticker = ticker,
                Name = model.Name,
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
