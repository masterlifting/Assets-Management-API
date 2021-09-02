using CommonServices.Models.Dto.Http;

using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Settings;
using IM.Gateways.Web.Companies.Api.Settings.Client;

using Microsoft.Extensions.Options;

using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Clients
{
    public class AnalyzerClient
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


        public async Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<RecommendationDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{recommendations}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<RecommendationDto>> GetRecommendationAsync(string ticker) => 
            await httpClient.GetFromJsonAsync<ResponseModel<RecommendationDto>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{recommendations}/{ticker}") ?? new();


        public async Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<RatingDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{ratings}?{pagination.QueryParams}") ?? new();
        public async Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker) => 
            await httpClient.GetFromJsonAsync<ResponseModel<RatingDto>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{ratings}/{ticker}") ?? new();


        public async Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination) => 
            await httpClient.GetFromJsonAsync<ResponseModel<PaginationResponseModel<CoefficientDto>>>
                ($"{settings.Schema}://{settings.Host}:{settings.Port}/{coefficients}/{ticker}?{pagination.QueryParams}") ?? new();
    }
}
