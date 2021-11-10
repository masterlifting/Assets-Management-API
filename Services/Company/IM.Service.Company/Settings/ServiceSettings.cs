using IM.Service.Common.Net.Models.Configuration;

using IM.Service.Company.Settings.Client;

namespace IM.Service.Company.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}
