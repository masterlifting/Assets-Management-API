using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Models.Dto;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.Agregators.Interfaces
{
    public interface IRatingDtoAgregator
    {
        Task<ResponseModel<RatingDto>> GetRatingAsync(string ticker);
        Task<ResponseModel<PaginationResponseModel<RatingDto>>> GetRatingsAsync(PaginationRequestModel pagination);
    }
}
