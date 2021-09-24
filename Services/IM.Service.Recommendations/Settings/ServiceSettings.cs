using CommonServices.Models;

using IM.Gateway.Recommendations.Settings.Client;

namespace IM.Gateway.Recommendations.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
