using IM.Service.Shared.Background;
using static IM.Service.Market.Enums;

namespace IM.Service.Market.Settings.Sections;

public class LoadTaskSettings : BackgroundTaskSettings
{
    public Sources[] Sources { get; set; } = Array.Empty<Sources>();
}