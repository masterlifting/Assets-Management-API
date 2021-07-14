using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations
{
    public class ReportsDtoAgregator : IReportsDtoAgregator
    {
        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetHistoryReportsAsync(string ticker, PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<ReportDto>> GetLastReportAsync(string ticker)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetLastReportsAsync(PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}
