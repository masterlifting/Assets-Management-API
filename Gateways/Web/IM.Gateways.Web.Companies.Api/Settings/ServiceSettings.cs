using CommonServices.Models;

using IM.Gateways.Web.Companies.Api.Settings.Client;

namespace IM.Gateways.Web.Companies.Api.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
