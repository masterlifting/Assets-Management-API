using CommonServices.HttpServices;

using Gateway.Api.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace Gateway.Api.Clients
{
    public class CompanyAnalyzerClient : RestClient
    {
        public CompanyAnalyzerClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.CompanyAnalyzer) { }
    }
}
