using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations
{
    public class CompaniesDtoAgregator : ICompaniesDtoAgregator
    {
        public IPricesDtoAgregator PricesDtoAgregators => throw new System.NotImplementedException();
        public IReportsDtoAgregator ReportsDtoAgregators => throw new System.NotImplementedException();
        public IAnalyzerDtoAgregator AnalyzerDtoAgregators => throw new System.NotImplementedException();

        public Task<ResponseModel<PaginationResponseModel<CompanyDto>>> GetCompaniesAsync(PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }
        public Task<ResponseModel<CompanyDto>> GetCompanyAsync(string ticker)
        {
            throw new System.NotImplementedException();
        }
        public Task<ResponseModel<string>> DeleteCompanyAsync(string ticker)
        {
            throw new System.NotImplementedException();
        }
    }
}
