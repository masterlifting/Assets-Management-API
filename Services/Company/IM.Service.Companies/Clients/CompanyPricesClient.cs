using CommonServices.HttpServices;
using Microsoft.Extensions.Options;

using System.Net.Http;
using IM.Service.Companies.Settings;

namespace IM.Service.Companies.Clients
{
    public class CompanyPricesClient : RestClient
    {
        public CompanyPricesClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
            : base(httpClient, settings.Value.ClientSettings.CompanyPrices) { }
    }
}
