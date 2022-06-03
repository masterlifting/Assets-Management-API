using IM.Service.Shared.Models.Configuration;
using IM.Service.Portfolio.Settings.Sections;

namespace IM.Service.Portfolio.Settings;

public class ServiceSettings
{
    public ClientSettings ClientSettings { get; set; } = null!;
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
}