using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IM.Gateways.Web.Company.Settings;
using IM.Gateways.Web.Company.Settings.Client;

namespace IM.Gateways.Web.Company.Clients
{
    public class PricesClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly HostModel settings;

        public PricesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ClientSettings.ClientCompaniesPrices;
        }


        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(FilterRequestModel filter, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{settings.Controller}?{filter.QueryParams}&{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<PriceDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{settings.Controller}/{ticker}?{filter.QueryParams}&{pagination.QueryParams}") ?? new();

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
