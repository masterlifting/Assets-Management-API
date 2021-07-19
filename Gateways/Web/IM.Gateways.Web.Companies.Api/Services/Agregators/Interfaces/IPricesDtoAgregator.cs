using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces
{
    public interface IPricesDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination);
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination);
    }
}
