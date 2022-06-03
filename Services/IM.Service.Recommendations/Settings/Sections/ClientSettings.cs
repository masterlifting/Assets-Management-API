using IM.Service.Shared.Models.Http;

namespace IM.Service.Recommendations.Settings.Sections;

public class ClientSettings
{
    public HostModel Market { get; set; } = null!;
    public HostModel Portfolio { get; set; } = null!;
}