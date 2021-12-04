using DataSetter.Settings;

using IM.Service.Common.Net.HttpServices;

using Microsoft.Extensions.Options;

using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;

namespace DataSetter.Clients;

public class CompanyClient : RestClient
{
    public CompanyClient(IMemoryCache cache, HttpClient httpClient, IOptions<ServiceSettings> settings)
        : base(cache, httpClient, settings.Value.ClientSettings.Company) { }
}