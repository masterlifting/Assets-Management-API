using CommonServices.Models;
using IM.Services.Company.Prices.Settings.Client;

namespace IM.Services.Company.Prices.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
}