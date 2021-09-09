using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Companies.Api.Clients;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.DtoServices
{
    public class PricesDtoAggregator
    {
        private readonly PricesClient httpClient;
        public PricesDtoAggregator(PricesClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetPricesAsync(filter, pagination);
        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) =>
            httpClient.GetPricesAsync(ticker, filter, pagination);
    }
}
