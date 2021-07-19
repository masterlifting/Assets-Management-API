using IM.Services.Companies.Reports.Api.Models;
using IM.Services.Companies.Reports.Api.Models.Dto;

using System.Threading.Tasks;

namespace IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces
{
    public interface IReportsDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination);
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, PaginationRequestModel pagination);
    }
}
