using CommonServices.HttpServices;

using IM.Gateway.Recommendations.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Gateway.Recommendations.Clients
{
    public class CompanyAnalyzerClient : RestClient
    {
        public CompanyAnalyzerClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.CompanyAnalyzer) { }
    }
}
