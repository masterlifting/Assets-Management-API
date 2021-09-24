using CommonServices.HttpServices;

using Gateway.Api.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace Gateway.Api.Clients
{
    public class CompaniesClient : RestClient
    {
        public CompaniesClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.Companies) { }
    }
}
