using IM.Service.Common.Net.HttpServices;

using IM.Service.Company.Analyzer.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Service.Company.Analyzer.Clients
{
    public class CompanyDataClient : RestClient
    {
        public CompanyDataClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.CompanyData) { }
    }
}
