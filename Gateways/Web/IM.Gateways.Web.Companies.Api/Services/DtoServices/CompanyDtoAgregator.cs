using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.DataAccess;
using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.DtoServices
{
    public class CompanyDtoAgregator
    {
        private readonly GatewaysContext context;
        private readonly PricesClient pricesClient;
        private readonly ReportsClient reportsClient;
        private readonly AnalyzerClient analyzerClient;

        public CompanyDtoAgregator(
            GatewaysContext context
            , PricesClient pricesClient
            , ReportsClient reportsClient
            , AnalyzerClient analyzerClient)
        {
            this.context = context;
            this.pricesClient = pricesClient;
            this.reportsClient = reportsClient;
            this.analyzerClient = analyzerClient;
        }
        public PricesDtoAgregator PricesDtoAgregator { get => new(pricesClient); }
        public ReportsDtoAgregator ReportsDtoAgregator { get => new(reportsClient); }
        public AnalyzerDtoAgregator AnalyzerDtoAgregator { get => new(analyzerClient); }

        public async Task<ResponseModel<PaginationResponseModel<CompanyDto>>> GetCompaniesAsync(PaginationRequestModel pagination)
        {
            var errors = Array.Empty<string>();

            var query = context.Companies.AsQueryable();
            int count = await query.CountAsync();

            var companies = await context.Companies
                .OrderBy(x => x.Name)
                .Skip((pagination.Page - 1) * pagination.Limit)
                .Take(pagination.Limit)
                .Select(x => new CompanyDto()
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
        public async Task<ResponseModel<CompanyDto>> GetCompanyAsync(string ticker)
        {
            var company = await context.Companies.FindAsync(ticker.ToUpperInvariant());

            return company is null
                ? new() { Errors = new string[1] { "company not found" } }
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
