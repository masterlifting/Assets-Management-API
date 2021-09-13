using CommonServices.Models.Dto.AnalyzerService;
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
    public class AnalyzerClient : IDisposable
    {
        private const string recommendations = "recommendations";
        private const string ratings = "ratings";
        private const string coefficients = "coefficients";

        private readonly HttpClient httpClient;
        private readonly HostModel settings;

        public AnalyzerClient(HttpClient httpClient, IOptions<ServiceSettings> options)
        {
            this.httpClient = httpClient;
            settings = options.Value.ClientSettings.ClientAnalyzer;
        }


        public async Task<ResponseModel<PaginationResponseModel<AnalyzerRecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<AnalyzerRecommendationDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{recommendations}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<AnalyzerRecommendationDto>> GetRecommendationAsync(string ticker) => 
            await httpClient.GetFromJsonAsync<ResponseModel<AnalyzerRecommendationDto>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{recommendations}/{ticker}") ?? new();


        public async Task<ResponseModel<PaginationResponseModel<AnalyzerRatingDto>>> GetRatingsAsync(PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<AnalyzerRatingDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{ratings}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<AnalyzerRatingDto>> GetRatingAsync(string ticker) => 
            await httpClient.GetFromJsonAsync<ResponseModel<AnalyzerRatingDto>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{ratings}/{ticker}") ?? new();


        public async Task<ResponseModel<PaginationResponseModel<AnalyzerCoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<AnalyzerCoefficientDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{coefficients}/{ticker}?{pagination.QueryParams}") ?? new();

        public void Dispose()
        {
            httpClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
