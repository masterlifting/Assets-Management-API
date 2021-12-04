using IM.Service.Common.Net.HttpServices;
using IM.Service.Company.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace IM.Service.Company.Clients;

public class CompanyAnalyzerClient : RestClient
{
    public CompanyAnalyzerClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> options)
        : base(cache, httpClient, options.Value.ClientSettings.CompanyAnalyzer) { }
}