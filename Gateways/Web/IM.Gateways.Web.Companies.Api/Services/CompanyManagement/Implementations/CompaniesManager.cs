using IM.Gateways.Web.Companies.Api.DataAccess;
using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.CompanyManagement.Interfaces;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Interfaces;

using System;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.CompanyManagement.Implementations
{
    public class CompaniesManager : ICompaniesManager
    {
        private readonly GatewaysContext context;
        private readonly IRabbitmqManager rabbitmqManager;

        public CompaniesManager(GatewaysContext context, IRabbitmqManager rabbitmqManager)
        {
            this.context = context;
            this.rabbitmqManager = rabbitmqManager;
        }

        public async Task<ResponseModel<string>> CreateCompanyAsync(CompanyModel company)
        {
            company.Ticker = company.Ticker.ToUpperInvariant();
            var ctxCompany = await context.Companies.FindAsync(company.Ticker);

            if (ctxCompany is not null)
                return new() { Errors = new string[1] { "this is company is already" } };

            string contextResultMessage = "failed";

            await context.Companies.AddAsync(new()
            {
                Ticker = company.Ticker.ToUpperInvariant(),
                Name = company.Name,
                Description = company.Description
            });

            if (await context.SaveChangesAsync() > 0)
            {
                contextResultMessage = "success";
                await rabbitmqManager.CreateCompanyAsync(company);
            }

            return new()
            {
                Data = $"created '{company.Name}' is {contextResultMessage}.",
            };
        }
        public async Task<ResponseModel<string>> UpdateCompanyAsync(string ticker, CompanyModel company)
        {
            var errors = Array.Empty<string>();
            string contextResultMessage = "failed";
            
            ticker = ticker.ToUpperInvariant();
            company.Ticker = ticker;

            var ctxCompany = await context.Companies.FindAsync(ticker);

            if (ctxCompany is null)
                return new() { Errors = new string[1] { "this is company not found" } };

            if (!company.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
                errors = new string[1] { "ticker modified  is denied" };

            ctxCompany.Name = company.Name;
            ctxCompany.Description = company.Description;

            if (await context.SaveChangesAsync() >= 0)
            {
                contextResultMessage = "success";
                await rabbitmqManager.UpdateCompanyAsync(company);
            }

            return new()
            {
                Data = $"updated '{company.Name}' is {contextResultMessage}",
                Errors = errors
            };
        }
        public async Task<ResponseModel<string>> DeleteCompanyAsync(string ticker)
        {
            ticker = ticker.ToUpperInvariant();
            var company = await context.Companies.FindAsync(ticker);

            if (company is null)
                return new() { Errors = new string[1] { "this is company not found" } };

            context.Companies.Remove(company);

            string contextResultMessage = "failed";
            if (await context.SaveChangesAsync() >= 0)
            {
                contextResultMessage = "success";
                await rabbitmqManager.DeleteCompanyAsync(ticker);
            }

            return new()
            {
                Data = $"deleted '{company.Name}' is {contextResultMessage}"
            };
        }
    }
}
