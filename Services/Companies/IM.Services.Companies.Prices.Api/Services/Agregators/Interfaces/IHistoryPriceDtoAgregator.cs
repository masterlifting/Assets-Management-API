using System.Threading.Tasks;
using IM.Services.Companies.Prices.Api.Models;
using IM.Services.Companies.Prices.Api.Models.Dto;

namespace IM.Services.Companies.Prices.Api.Services.Agregators.Interfaces
{
    public interface IHistoryPriceDtoAgregator
    {
        Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetPricesAsync(string ticker, PaginationRequestModel pagination);
    }
}