using IM.Gateways.Web.Companies.Api.DataAccess;
using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.CompanyManagement.Interfaces;
using IM.Gateways.Web.Companies.Api.Services.RabbitMqManagement.Implementations;
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
            var _company = await context.Companies.FindAsync(company.Ticker.ToUpperInvariant());

            if (_company is not null)
                return new() { Errors = new string[1] { "this is company is already" } };

            await context.Companies.AddAsync(new()
            {
                Ticker = company.Ticker.ToUpperInvariant(),
                Name = company.Name,
                Description = company.Description
            });

            rabbitmqManager.CreateCompany(company);

            string contextResultMessage = "failed";
            string mqResultMessage = "failed";

            //if (await context.SaveChangesAsync() >= 0)
            //{
            //    contextResultMessage = "success";

            //    if (new RabbitMqCompanyProvider().CreateCompany(company))
            //        mqResultMessage = "success";
            //}

            return new()
            {
                Data = $"Company: {company.Name} is {contextResultMessage}. Ticker send to rabbit is {mqResultMessage}.",
            };
        }
        public async Task<ResponseModel<string>> EditCompanyAsync(string ticker, CompanyModel company)
        {
            var errors = Array.Empty<string>();
            var _company = await context.Companies.FindAsync(ticker.ToUpperInvariant());

            if (_company is null)
                return new() { Errors = new string[1] { "this is company not found" } };

            if(!company.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase))
                errors = new string[1] { "ticker modified  is denied" };

            _company.Name = company.Name;
            _company.Description = company.Description;

            if (await context.SaveChangesAsync() >= 0)
            {

            }

            return new()
            {
                Data = $"company: {company.Name} is edited",
                Errors = errors
            };
        }
        public async Task<ResponseModel<string>> DeleteCompanyAsync(string ticker)
        {
            var company = await context.Companies.FindAsync(ticker.ToUpperInvariant());

            if (company is null)
                return new() { Errors = new string[1] { "this is company not found" } };

            context.Companies.Remove(company);

            if (await context.SaveChangesAsync() >= 0)
            {

            }

            return new()
            {
                Data = $"company: {company.Name} is deleted",
            };
        }
    }
}
