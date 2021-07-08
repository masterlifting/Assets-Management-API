using System.Threading.Tasks;
using IM.Services.Companies.Reports.Api.Models;
using IM.Services.Companies.Reports.Api.Models.Dto;

namespace IM.Services.Companies.Reports.Api.Services.Agregators.Interfaces
{
    public interface ILastReportDtoAgregator
    {
        Task<ResponseModel<ReportDto>> GetReportAsync(string ticker);
        Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination);
    }
}