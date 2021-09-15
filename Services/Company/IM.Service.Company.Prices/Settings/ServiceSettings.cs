using CommonServices.Models;
using IM.Service.Company.Prices.Settings.Client;

namespace IM.Service.Company.Prices.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}