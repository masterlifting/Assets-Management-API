using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Clients
{
    public class ReportsClient
    {
        private const string reports = "reports";

        private readonly HttpClient httpClient;
        private readonly ClientSettings settings;

        public ReportsClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ReportsSettings;
        }


        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<ReportDto>>>
                ($"{settings.Sсhema}://{settings.Host}:{settings.Port}/{reports}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<PaginationResponseModel<ReportDto>>> GetReportsAsync(string ticker, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<ReportDto>>>
                ($"{settings.Sсhema}://{settings.Host}:{settings.Port}/{reports}/{ticker}?{pagination.QueryParams}") ?? new();
    }
}
