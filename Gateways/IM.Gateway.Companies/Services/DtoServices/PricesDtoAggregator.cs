using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using System.Threading.Tasks;
using IM.Gateway.Companies.Clients;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class PricesDtoAggregator
    {
        private readonly PricesClient httpClient;
        public PricesDtoAggregator(PricesClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetAsync(filter, pagination);
        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetAsync(ticker, filter, pagination);
    }
}
