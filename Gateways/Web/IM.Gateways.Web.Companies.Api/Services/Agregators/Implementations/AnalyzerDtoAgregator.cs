using IM.Gateways.Web.Companies.Api.Clients;
using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;

using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations
{
    public class AnalyzerDtoAgregator : IAnalyzerDtoAgregator
    {
        private readonly AnalyzerClient httpClient;
        public AnalyzerDtoAgregator(AnalyzerClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination) =>
            httpClient.GetRecommendationsAsync(pagination);
        public Task<ResponseModel<RecommendationDto>> GetRecommendationAsync(string ticker) => httpClient.GetRecommendationAsync(ticker);

        public Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination) =>
            httpClient.GetRatingsAsync(pagination);
        public Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker) => httpClient.GetRatingAsync(ticker);

        public Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination) =>
            httpClient.GetCoefficientsAsync(ticker, pagination);
    }
}
