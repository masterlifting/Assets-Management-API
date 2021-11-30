using DataSetter.Settings;

using IM.Service.Common.Net.HttpServices;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace DataSetter.Clients;

public class CompanyClient : RestClient
{
    public CompanyClient(HttpClient httpClient, IOptions<ServiceSettings> settings)
        : base(httpClient, settings.Value.ClientSettings.Company) { }
}