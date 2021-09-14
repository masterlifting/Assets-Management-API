using CommonServices.Models.Dto.AnalyzerService;
using CommonServices.Models.Dto.Http;
using System.Threading.Tasks;
using IM.Gateway.Companies.Clients;

namespace IM.Gateway.Companies.Services.DtoServices
{
    public class AnalyzerDtoAggregator
    {
        private readonly AnalyzerClient httpClient;
        public AnalyzerDtoAggregator(AnalyzerClient httpClient) => this.httpClient = httpClient;

        public Task<ResponseModel<PaginationResponseModel<AnalyzerRecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination) =>
            httpClient.GetRecommendationsAsync(pagination);
        public Task<ResponseModel<AnalyzerRecommendationDto>> GetRecommendationAsync(string ticker) => httpClient.GetRecommendationAsync(ticker);

        public Task<ResponseModel<PaginationResponseModel<AnalyzerRatingDto>>> GetRatingsAsync(PaginationRequestModel pagination) =>
            httpClient.GetRatingsAsync(pagination);
        public Task<ResponseModel<AnalyzerRatingDto>> GetRatingAsync(string ticker) => httpClient.GetRatingAsync(ticker);

        public Task<ResponseModel<PaginationResponseModel<AnalyzerCoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination) =>
            httpClient.GetCoefficientsAsync(ticker, pagination);
    }
}
