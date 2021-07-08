using IM.Services.Analyzer.Api.Models.Http;
using IM.Services.Analyzer.Api.Models.Dto;
using System.Threading.Tasks;

namespace IM.Services.Analyzer.Api.Services.Agregators.Interfaces
{
    public interface ICoefficientDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<CoefficientDto>>> GetCoefficientsAsync(string ticker, PaginationRequestModel pagination);
    }
}
