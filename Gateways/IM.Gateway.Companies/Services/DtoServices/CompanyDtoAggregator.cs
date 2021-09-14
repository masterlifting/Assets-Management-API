using CommonServices.Models.Dto.Http;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;
using IM.Gateway.Companies.Clients;
using IM.Gateway.Companies.DataAccess;
using IM.Gateway.Companies.Models.Dto;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class CompanyDtoAggregator
    {
        private readonly DatabaseContext context;
        private readonly PricesClient pricesClient;
        private readonly ReportsClient reportsClient;
        private readonly AnalyzerClient analyzerClient;

        public CompanyDtoAggregator(
            DatabaseContext context
            , PricesClient pricesClient
            , ReportsClient reportsClient
            , AnalyzerClient analyzerClient)
        {
            this.context = context;
            this.pricesClient = pricesClient;
            this.reportsClient = reportsClient;
            this.analyzerClient = analyzerClient;
        }
        public PricesDtoAggregator PricesDtoAggregator => new(pricesClient);
        public ReportsDtoAggregator ReportsDtoAggregator => new(reportsClient);
        public AnalyzerDtoAggregator AnalyzerDtoAggregator => new(analyzerClient);

        public async Task<ResponseModel<PaginationResponseModel<CompanyGetDto>>> GetCompaniesAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var query = context.Companies.AsQueryable();
            var count = await query.CountAsync();

            var companies = await context.Companies
                .OrderBy(x => x.Name)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
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
        public async Task<ResponseModel<CompanyGetDto>> GetCompanyAsync(string ticker)
        {
            var company = await context.Companies.FindAsync(ticker.ToUpperInvariant());

            return company is null
                ? new() { Errors = new[] { "company not found" } }
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
    }
}
