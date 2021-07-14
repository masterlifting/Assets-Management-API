using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations
{
    public class AnalyzerDtoAgregator : IAnalyzerDtoAgregator
    {
        public Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<RecommendationDto>> GetRecommendationAsync(string ticker)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}
