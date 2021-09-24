using CommonServices.HttpServices;

using Gateway.Api.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace Gateway.Api.Clients
{
    public class CompanyPricesClient : RestClient
    {
        public CompanyPricesClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.CompanyPrices) { }
    }
}
