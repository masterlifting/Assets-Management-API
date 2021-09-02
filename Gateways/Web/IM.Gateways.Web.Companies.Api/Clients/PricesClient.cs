using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Companies.Api.Settings;
using IM.Gateways.Web.Companies.Api.Settings.Client;

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
        private readonly HostModel settings;

        public PricesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ClientSettings.ClientCompaniesPrices;
        }


        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{prices}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{prices}/{ticker}?{pagination.QueryParams}") ?? new();
    }
}
