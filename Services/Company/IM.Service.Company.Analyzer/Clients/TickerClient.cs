using CommonServices.Models.Dto.Http;

using IM.Service.Company.Analyzer.Settings.Client;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IM.Service.Company.Analyzer.Clients
{
    public class TickerClient<T> : IDisposable where T : class
    {
        private readonly HttpClient httpClient;
        private readonly HostModel settings;

        protected TickerClient(HttpClient httpClient, HostModel settings)
        {
            this.httpClient = httpClient;
            this.settings = settings;
        }

        public async Task<ResponseModel<PaginationResponseModel<T>>> GetAsync(string ticker,FilterRequestModel filter, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<T>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{settings.Controller}/{ticker}?{filter.QueryParams}&{pagination.QueryParams}") ??
            new();

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
