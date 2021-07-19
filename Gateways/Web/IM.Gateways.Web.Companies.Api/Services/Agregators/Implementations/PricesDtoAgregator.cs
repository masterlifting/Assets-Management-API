using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations
{
    public class PricesDtoAgregator : IPricesDtoAgregator
    {
        private readonly PricesClient httpClient;
        public PricesDtoAgregator(PricesClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination) =>
            httpClient.GetPricesAsync(ticker, pagination);

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination) =>
            httpClient.GetPricesAsync(pagination);
    }
}
