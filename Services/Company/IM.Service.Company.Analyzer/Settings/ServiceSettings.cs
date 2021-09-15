using CommonServices.Models;
using IM.Service.Company.Analyzer.Settings.Client;

namespace IM.Service.Company.Analyzer.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
