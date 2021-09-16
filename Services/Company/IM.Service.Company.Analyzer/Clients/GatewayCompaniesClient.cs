using CommonServices.HttpServices;
using CommonServices.Models.Dto;

using IM.Service.Company.Analyzer.Settings;

using Microsoft.Extensions.Options;

using System.Net.Http;

namespace IM.Service.Company.Analyzer.Clients
{
    public class GatewayCompaniesClient : PaginationRequestClient<StockSplitDto>
    {
        public GatewayCompaniesClient(HttpClient httpClient, IOptions<ServiceSettings> options)
            : base(httpClient, options.Value.ClientSettings.GatewayCompanies) { }
    }
}
