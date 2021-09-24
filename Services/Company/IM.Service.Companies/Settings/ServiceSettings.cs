using CommonServices.Models;
using IM.Service.Companies.Settings.Client;

namespace IM.Service.Companies.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
