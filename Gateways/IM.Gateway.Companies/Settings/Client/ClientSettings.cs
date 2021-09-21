using CommonServices.Models.Http;

namespace IM.Gateway.Companies.Settings.Client
{
    public class ClientSettings
    {
        public HostModel Prices { get; set; } = null!;
        public HostModel Reports { get; set; } = null!;
        public HostModel Analyzer { get; set; } = null!;
    }
}
