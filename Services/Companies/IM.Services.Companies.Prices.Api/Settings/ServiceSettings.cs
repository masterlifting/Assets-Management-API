using IM.Services.Companies.Prices.Api.Settings.Client;
using IM.Services.Companies.Prices.Api.Settings.Mq;

namespace IM.Services.Companies.Prices.Api.Settings
{
    public class ServiceSettings
    {
        public MoexSettings MoexSettings { get; set; }
        public TdAmeritradeSettings TdAmeritradeSettings { get; set; }
        public MqQueueSettings QueueCompaniesPrices { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
}