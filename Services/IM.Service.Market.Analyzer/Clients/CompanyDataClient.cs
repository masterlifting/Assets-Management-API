using IM.Service.Common.Net.HttpServices;
using Microsoft.Extensions.Options;

using System.Net.Http;
using IM.Service.Market.Analyzer.Settings;
using Microsoft.Extensions.Caching.Memory;

namespace IM.Service.Market.Analyzer.Clients;

public class CompanyDataClient : RestClient
{
    public CompanyDataClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> options)
        : base(cache, httpClient, options.Value.ClientSettings.CompanyData) { }
}