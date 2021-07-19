﻿using IM.Gateways.Web.Companies.Api.Models.Dto.State;
using IM.Gateways.Web.Companies.Api.Models.Http;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Management.Interfaces
{
    public interface ICompaniesManager
    {
        Task<ResponseModel<string>> DeleteCompanyAsync(string ticker);
        Task<ResponseModel<string>> CreateCompanyAsync(CompanyModel company);
        Task<ResponseModel<string>> EditCompanyAsync(string ticker, CompanyModel company);
    }
}
