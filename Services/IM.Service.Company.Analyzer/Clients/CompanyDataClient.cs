using IM.Service.Common.Net.HttpServices;

using IM.Service.Company.Analyzer.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace IM.Service.Company.Analyzer.Clients;

public class CompanyDataClient : RestClient
{
    public CompanyDataClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> options)
        : base(cache, httpClient, options.Value.ClientSettings.CompanyData) { }
}