using IM.Service.Common.Net.Models.Configuration;
using IM.Service.Company.Data.Settings.Client;

namespace IM.Service.Company.Data.Settings
{
    public class ServiceSettings
    {
        public ClientSettings ClientSettings { get; set; } = null!;
        public ConnectionStrings ConnectionStrings { get; set; } = null!;
    }
}