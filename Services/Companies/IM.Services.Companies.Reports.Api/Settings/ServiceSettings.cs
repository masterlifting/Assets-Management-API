using IM.Services.Companies.Reports.Api.Settings.Client;
using IM.Services.Companies.Reports.Api.Settings.Connection;
using IM.Services.Companies.Reports.Api.Settings.Mq;

namespace IM.Services.Companies.Reports.Api.Settings
{
    public class ServiceSettings
    {
        public ConnectionStrings ConnectionStrings { get; set; }
        public ClientSettings ClientSettings { get; set; }
        public MqSettings MqSettings { get; set; }
    }
}