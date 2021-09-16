using CommonServices.Models.Dto.CompanyAnalyzer;
using CommonServices.Models.Dto.Http;

using IM.Gateway.Companies.Settings;

using Microsoft.Extensions.Options;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommonServices.Models.Http;

namespace IM.Gateway.Companies.Clients
{
    public class AnalyzerClient : IDisposable
    {
        private const string ratings = "ratings";
        private const string coefficients = "coefficients";

        private readonly HttpClient httpClient;
        private readonly HostModel settings;

        public AnalyzerClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ClientSettings.CompanyAnalyzer;
        }


        public async Task<ResponseModel<PaginationResponseModel<CompanyAnalyzerRatingDto>>> GetRatingsAsync(PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<CompanyAnalyzerRatingDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{ratings}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<CompanyAnalyzerRatingDto>> GetRatingAsync(string ticker) =>
            await httpClient.GetFromJsonAsync<ResponseModel<CompanyAnalyzerRatingDto>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{ratings}/{ticker}") ?? new();
        public async Task<ResponseModel<PaginationResponseModel<CompanyAnalyzerCoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination) =>
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<CompanyAnalyzerCoefficientDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{coefficients}/{ticker}?{pagination.QueryParams}") ?? new();

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
