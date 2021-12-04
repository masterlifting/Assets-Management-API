using IM.Service.Common.Net.HttpServices;
using IM.Service.Company.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace IM.Service.Company.Clients;

public class CompanyDataClient : RestClient
{
    public CompanyDataClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> settings)
        : base(cache, httpClient, settings.Value.ClientSettings.CompanyData) { }
}