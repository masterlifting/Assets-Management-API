using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using System.Threading.Tasks;
using IM.Gateway.Companies.Clients;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class ReportsDtoAggregator
    {
        private readonly ReportsClient httpClient;
        public ReportsDtoAggregator(ReportsClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetAsync(filter, pagination);

        public Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetAsync(ticker, filter, pagination);

    }
}
