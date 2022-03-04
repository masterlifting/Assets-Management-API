using IM.Service.Common.Net.Models.Configuration;

namespace IM.Service.Data.Settings;

public class ServiceSettings
{
    public ClientSettings ClientSettings { get; set; } = null!;
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
}