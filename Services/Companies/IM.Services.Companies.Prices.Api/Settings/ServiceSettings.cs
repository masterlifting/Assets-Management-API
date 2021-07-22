using IM.Services.Companies.Prices.Api.Settings.Client;
using IM.Services.Companies.Prices.Api.Settings.Connection;
using IM.Services.Companies.Prices.Api.Settings.Mq;

namespace IM.Services.Companies.Prices.Api.Settings
{
    public class ServiceSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public ClientSettings ClientSettings { get; set; }
        public MqSettings MqSettings { get; set; }
    }
}