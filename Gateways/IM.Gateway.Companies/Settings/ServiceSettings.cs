using CommonServices.Models;
using IM.Gateway.Companies.Settings.Client;

namespace IM.Gateway.Companies.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
