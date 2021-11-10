using DataSetter.Settings;

using IM.Service.Common.Net.HttpServices;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace DataSetter.Clients
{
    public class CompanyAnalyzerClient : RestClient
    {
        public CompanyAnalyzerClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.CompanyAnalyzer) { }
    }
}
