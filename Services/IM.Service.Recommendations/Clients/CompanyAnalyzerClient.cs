using IM.Service.Recommendations.Settings;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Net.Http;

using static IM.Service.Common.Net.Helpers.HttpHelper;

namespace IM.Service.Recommendations.Clients;

public class CompanyAnalyzerClient : RestClient
{
    public CompanyAnalyzerClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> options)
        : base(cache, httpClient, options.Value.ClientSettings.CompanyAnalyzer) { }
}