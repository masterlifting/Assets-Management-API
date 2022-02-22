using IM.Service.Common.Net.Models.Configuration;
using IM.Service.Market.Analyzer.Settings.Client;

namespace IM.Service.Market.Analyzer.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
