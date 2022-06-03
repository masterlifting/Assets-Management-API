using IM.Service.Shared.Models.Configuration;
using IM.Service.Market.Settings.Sections;

namespace IM.Service.Market.Settings;

public class ServiceSettings
{
    public ClientSettings ClientSettings { get; set; } = null!;
    public ConnectionStrings ConnectionStrings { get; set; } = null!;
    public LoadDataSettings LoadData { get; set; } = null!;
    public ComputeDataSettings ComputeData { get; set; } = null!;
}