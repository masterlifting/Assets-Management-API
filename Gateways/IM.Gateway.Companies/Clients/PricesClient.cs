using CommonServices.HttpServices;

using IM.Gateway.Companies.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Gateway.Companies.Clients
{
    public class PricesClient : RestClient
    {
        public PricesClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.Prices) { }
    }
}
