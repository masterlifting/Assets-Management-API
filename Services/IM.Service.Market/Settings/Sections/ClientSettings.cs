using IM.Service.Market.Models.Settings;
using IM.Service.Shared.Models.Configuration;

namespace IM.Service.Market.Settings.Sections;

public class ClientSettings
{
    public HostModel Moex { get; set; } = null!;
    public HostModel TdAmeritrade { get; set; } = null!;
    public InvestingModel Investing { get; set; } = null!;
}