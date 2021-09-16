using CommonServices.HttpServices;
using CommonServices.Models.Dto;

using IM.Gateway.Companies.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Gateway.Companies.Clients
{
    public class PricesClient : PaginationRequestClient<PriceDto>
    {
        public PricesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.CompanyPrices) { }
    }
}
