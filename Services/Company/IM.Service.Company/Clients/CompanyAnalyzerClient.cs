using IM.Service.Common.Net.HttpServices;

using Microsoft.Extensions.Options;

using System.Net.Http;
using IM.Service.Company.Settings;

namespace IM.Service.Company.Clients
{
    public class CompanyAnalyzerClient : RestClient
    {
        public CompanyAnalyzerClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.CompanyAnalyzer) { }
    }
}
