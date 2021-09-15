using CommonServices.HttpServices;
using IM.Service.Company.Analyzer.Settings;

using Microsoft.Extensions.Options;

namespace IM.Service.Company.Analyzer.Services.HealthCheck
{
    public class CompanyPricesHealthCheck : ExternalEndpointHealthCheck
    {
        protected CompanyPricesHealthCheck(IOptions<ServiceSettings> options) : base(options.Value.ClientSettings.CompanyPrices.Host) { }
    }
}