using CommonServices.Models;

using IM.Services.Companies.Prices.Api.Settings.Client;

namespace IM.Services.Companies.Prices.Api.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
}