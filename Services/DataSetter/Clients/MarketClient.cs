using DataSetter.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using System.Net.Http;
using static IM.Service.Shared.Helpers.HttpHelper;

namespace DataSetter.Clients;

public class MarketClient : RestClient
{
    public MarketClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> settings)
        : base(cache, httpClient, settings.Value.ClientSettings.Market) { }
}