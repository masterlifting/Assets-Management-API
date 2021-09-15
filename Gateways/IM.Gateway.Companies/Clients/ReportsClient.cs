using CommonServices.Models.Dto;
using CommonServices.Models.Dto.Http;
using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using IM.Gateway.Companies.Settings;
using IM.Gateway.Companies.Settings.Client;

namespace IM.Gateway.Companies.Clients
{
    public class ReportsClient : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly HostModel settings;

        public ReportsClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ClientSettings.CompanyReports;
        }

        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(FilterRequestModel filter, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<ReportDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{settings.Controller}?{filter.QueryParams}&{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, FilterRequestModel filter, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<ReportDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{settings.Controller}/{ticker}?{filter.QueryParams}&{pagination.QueryParams}") ?? new();

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
