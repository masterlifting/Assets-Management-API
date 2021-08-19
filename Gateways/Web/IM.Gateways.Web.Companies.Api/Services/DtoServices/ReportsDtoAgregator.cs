using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.DtoServices
{
    public class ReportsDtoAgregator
    {
        private readonly ReportsClient httpClient;
        public ReportsDtoAgregator(ReportsClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, PaginationRequestModel pagination) =>
            httpClient.GetReportsAsync(ticker, pagination);

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination) =>
            httpClient.GetReportsAsync(pagination);
    }
}
