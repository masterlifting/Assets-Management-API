using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces
{
    public interface IAnalyzerDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination);
        Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination);
        Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker);
        Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination);
        Task<ResponseModel<RecommendationDto>> GetRecommendationAsync(string ticker);
    }
}
