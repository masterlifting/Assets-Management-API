using CommonServices.Models;
using IM.Service.Company.Reports.Settings.Client;

namespace IM.Service.Company.Reports.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}