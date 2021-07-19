using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces
{
    public interface IReportsDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination);
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, PaginationRequestModel pagination);
    }
}
