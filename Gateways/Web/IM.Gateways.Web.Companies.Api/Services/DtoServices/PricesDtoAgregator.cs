using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.DtoServices
{
    public class PricesDtoAgregator
    {
        private readonly PricesClient httpClient;
        public PricesDtoAgregator(PricesClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination) =>
            httpClient.GetPricesAsync(ticker, pagination);

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination) =>
            httpClient.GetPricesAsync(pagination);
    }
}
