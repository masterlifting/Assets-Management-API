using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces
{
    public interface IReportsDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetLastReportsAsync(PaginationRequestModel pagination);
        Task<ResponseModel<ReportDto>> GetLastReportAsync(string ticker);
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetHistoryReportsAsync(string ticker, PaginationRequestModel pagination);
    }
}
