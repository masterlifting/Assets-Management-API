using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Clients
{
    public class PricesClient
    {
        private const string prices = "prices";

        private readonly HttpClient httpClient;
        private readonly ClientSettings settings;

        public PricesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.PricesSettings;
        }


        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Sсhema}://{settings.Host}:{settings.Port}/{prices}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Sсhema}://{settings.Host}:{settings.Port}/{prices}/{ticker}?{pagination.QueryParams}") ?? new();
    }
}
