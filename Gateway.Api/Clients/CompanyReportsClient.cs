using CommonServices.HttpServices;

using Gateway.Api.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace Gateway.Api.Clients
{
    public class CompanyReportsClient : RestClient
    {
        public CompanyReportsClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.CompanyReports) { }
    }
}
