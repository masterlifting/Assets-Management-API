using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces
{
    public interface ICompaniesDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<CompanyDto>>> GetCompaniesAsync(PaginationRequestModel pagination);
        Task<ResponseModel<CompanyDto>> GetCompanyAsync(string ticker);

        IPricesDtoAgregator PricesDtoAgregator { get; }
        IReportsDtoAgregator ReportsDtoAgregator { get; }
        IAnalyzerDtoAgregator AnalyzerDtoAgregator { get; }
    }
}
