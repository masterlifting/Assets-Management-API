using IM.Service.Recommendations.Settings;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Net.Http;

using static IM.Service.Shared.Helpers.HttpHelper;

namespace IM.Service.Recommendations.Clients;

public class PortfolioClient : RestClient
{
    public PortfolioClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> options)
        : base(cache, httpClient, options.Value.ClientSettings.Portfolio) { }
}