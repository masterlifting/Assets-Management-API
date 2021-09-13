using CommonServices.Models;
using IM.Services.Company.Reports.Settings.Client;

namespace IM.Services.Company.Reports.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; }
        public ConnectionStrings ConnectionStrings { get; set; }
    }
}