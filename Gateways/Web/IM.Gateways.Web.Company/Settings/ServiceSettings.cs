using CommonServices.Models;
using IM.Gateways.Web.Company.Settings.Client;

namespace IM.Gateways.Web.Company.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
