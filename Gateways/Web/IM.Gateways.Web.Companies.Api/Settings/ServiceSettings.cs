using IM.Gateways.Web.Companies.Api.Settings.Client;
using IM.Gateways.Web.Companies.Api.Settings.Rabbitmq;

namespace IM.Gateways.Web.Companies.Api.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientCompaniesPricesSettings { get; set; } = null!;
        public ClientSettings ClientCompaniesReportsSettings { get; set; } = null!;
        public ClientSettings ClientAnalyzerSettings { get; set; } = null!;
        public RabbitmqSettings RabbitmqSettings { get; set; } = null!;
    }
}
