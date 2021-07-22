using IM.Gateways.Web.Companies.Api.Settings.Client;
using IM.Gateways.Web.Companies.Api.Settings.Connection;
using IM.Gateways.Web.Companies.Api.Settings.Mq;

namespace IM.Gateways.Web.Companies.Api.Settings
{
    public class ServiceSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
        public ClientSettings ClientSettings { get; set; } = null!;
        public MqSettings MqSettings { get; set; } = null!;
    }
}
