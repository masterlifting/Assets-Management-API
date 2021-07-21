using IM.Services.Companies.Reports.Api.Settings.Rabbitmq;
using IM.Services.Companies.Reports.Api.Settings.Reports.Investing;

namespace IM.Services.Companies.Reports.Api.Settings
{
    public class ServiceSettings
    {
        public InvestingSettings InvestingSettings { get; set; }
        public RabbitmqSettings RabbitmqSettings { get; set; }
    }
}