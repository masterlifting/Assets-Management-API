using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces
{
    public interface IPricesDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetLastPricesAsync(PaginationRequestModel pagination);
        Task<ResponseModel<PriceDto>> GetLastPriceAsync(string ticker);
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetHistoryPricesAsync(string ticker, PaginationRequestModel pagination);
    }
}
