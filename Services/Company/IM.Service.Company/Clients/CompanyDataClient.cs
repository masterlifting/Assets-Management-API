using IM.Service.Common.Net.HttpServices;
using IM.Service.Company.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Service.Company.Clients;

public class CompanyDataClient : RestClient
{
    public CompanyDataClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
        : base(httpClient, settings.Value.ClientSettings.CompanyData) { }
}