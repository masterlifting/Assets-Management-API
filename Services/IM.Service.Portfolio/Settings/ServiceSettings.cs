using IM.Service.Common.Net.Models.Configuration;
using IM.Service.Portfolio.Settings.Client;

namespace IM.Service.Portfolio.Settings;

public class ServiceSettings
{
    public ClientSettings ClientSettings { get; set; } = null!;
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
}