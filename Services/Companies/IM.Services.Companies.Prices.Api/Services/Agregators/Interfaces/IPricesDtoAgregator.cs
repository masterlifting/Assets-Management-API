using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;

using System.Threading.Tasks;

namespace IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces
{
    public interface IPricesDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(PaginationRequestModel pagination);
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination);
    }
}
