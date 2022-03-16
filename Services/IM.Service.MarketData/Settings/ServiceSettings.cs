using IM.Service.Common.Net.Models.Configuration;

namespace IM.Service.MarketData.Settings;

public class ServiceSettings
{
    public ClientSettings ClientSettings { get; set; } = null!;
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
}