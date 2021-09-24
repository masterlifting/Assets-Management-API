using CommonServices.HttpServices;

using IM.Service.Company.Analyzer.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Service.Company.Analyzer.Clients
{
    public class CompaniesClient : RestClient
    {
        public CompaniesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.Companies) { }
    }
}
