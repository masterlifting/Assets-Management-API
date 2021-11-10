using IM.Service.Common.Net.HttpServices;
using Microsoft.Extensions.Options;

using System.Net.Http;
using IM.Service.Company.Settings;

namespace IM.Service.Company.Clients
{
    public class CompanyDataClient : RestClient
    {
        public CompanyDataClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.CompanyData) { }
    }
}
