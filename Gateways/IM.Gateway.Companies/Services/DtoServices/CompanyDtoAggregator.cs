using CommonServices.Models.Dto.Http;

using IM.Gateway.Companies.Clients;
using IM.Gateway.Companies.DataAccess.Entities;
using IM.Gateway.Companies.DataAccess.Repository;
using IM.Gateway.Companies.Models.Dto;
using IM.Gateway.Companies.Services.RabbitServices;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class CompanyDtoAggregator
    {
        private readonly PricesClient pricesClient;
        private readonly ReportsClient reportsClient;
        private readonly AnalyzerClient analyzerClient;
        private readonly RepositorySet<Company> repository;
        private readonly RabbitCrudService rabbitCrudService;

        public CompanyDtoAggregator(
            RepositorySet<Company> repository
            , PricesClient pricesClient
            , ReportsClient reportsClient
            , AnalyzerClient analyzerClient
            , RabbitCrudService rabbitCrudService)
        {
            this.pricesClient = pricesClient;
            this.reportsClient = reportsClient;
            this.analyzerClient = analyzerClient;
            this.repository = repository;
            this.rabbitCrudService = rabbitCrudService;
        }
        public PricesDtoAggregator PricesDtoAggregator => new(pricesClient);
        public ReportsDtoAggregator ReportsDtoAggregator => new(reportsClient);
        public AnalyzerDtoAggregator AnalyzerDtoAggregator => new(analyzerClient);

        public async Task<ResponseModel<PaginationResponseModel<CompanyGetDto>>> GetAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var count = await repository.GetDbSetBy<Company>().CountAsync();

            var paginatedResult = repository.QueryPaginator(pagination, x => x.Name);

            var companies = await paginatedResult
                .Select(x => new CompanyGetDto()
                {
                    Name = x.Name,
                    Ticker = x.Ticker,
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
            var company = await repository.FindAsync(ticker.ToUpperInvariant());

            return company is null
                ? new() { Errors = new[] { "model not found" } }
                : new()
                {
                    Data = new()
                    {
                        Ticker = company.Ticker,
                        Name = company.Name,
                        Description = company.Description
                    }
                };
        }
        public async Task<ResponseModel<string>> CreateAsync(CompanyPostDto model)
        {
            var ctxCompany = new Company()
            {
                Ticker = model.Ticker!,
                Name = model.Name,
                Description = model.Description
            };

            var (errors, createdCompany) = await repository.CreateAsync(ctxCompany, model.Name);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.CreateCompany(new()
            {
                Name = createdCompany!.Name,
                Ticker = createdCompany.Ticker,
                Description = createdCompany.Description,
                PriceSourceTypeId = model.PriceSourceTypeId,
                ReportSourceTypeId = model.ReportSourceTypeId,
                ReportSourceValue = model.ReportSourceValue
            });

            return new ResponseModel<string> { Data = $"'{model.Name}' created" };
        }
        public async Task<ResponseModel<string>> UpdateAsync(string ticker, CompanyPostDto model)
        {
            var ctxCompany = new Company()
            {
                Ticker = ticker,
                Name = model.Name,
                Description = model.Description
            };

            var (errors, updatedCompany) = await repository.UpdateAsync(ctxCompany, ticker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.UpdateCompany(new()
            {
                Name = updatedCompany!.Name,
                Ticker = updatedCompany.Ticker,
                Description = updatedCompany.Description,
                PriceSourceTypeId = model.PriceSourceTypeId,
                ReportSourceTypeId = model.ReportSourceTypeId,
                ReportSourceValue = model.ReportSourceValue
            });

            return new ResponseModel<string> { Data = $"'{model.Name}' updated" };
        }
        public async Task<ResponseModel<string>> DeleteAsync(string ticker)
        {
            var errors = await repository.DeleteAsync(ticker.ToUpperInvariant().Trim(), ticker);

            if (errors.Any())
                return new ResponseModel<string> { Errors = errors };

            rabbitCrudService.DeleteCompany(ticker);

            return new ResponseModel<string> { Data = $"'{ticker}' deleted" };
        }
    }
}
