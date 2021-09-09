using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Companies.Api.Clients;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.DtoServices
{
    public class ReportsDtoAggregator
    {
        private readonly ReportsClient httpClient;
        public ReportsDtoAggregator(ReportsClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetReportsAsync(filter, pagination);

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetReportsAsync(ticker, filter, pagination);

    }
}
