using IM.Gateways.Web.Companies.Api.Models.Dto;
using IM.Gateways.Web.Companies.Api.Models.Http;
using IM.Gateways.Web.Companies.Api.Services.Agregators.Interfaces;
using System.Threading.Tasks;

namespace IM.Gateways.Web.Companies.Api.Services.Agregators.Implementations
{
    public class PricesDtoAgregator : IPricesDtoAgregator
    {
        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetHistoryPricesAsync(string ticker, PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<PriceDto>> GetLastPriceAsync(string ticker)
        {
            throw new System.NotImplementedException();
        }

        public Task<ResponseModel<PaginationResponseModel<PriceDto>>> GetLastPricesAsync(PaginationRequestModel pagination)
        {
            throw new System.NotImplementedException();
        }
    }
}
