using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Models.Dto;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.Agregators.Interfaces
{
    public interface IRecommendationDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<RecommendationDto>>> GetRecommendationsAsync(PaginationRequestModel pagination);
        Task<ResponseModel<RecommendationDto>> GetRecommendationAsync(string ticker);
    }
}
