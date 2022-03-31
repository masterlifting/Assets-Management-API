using IM.Service.Common.Net.Models.Dto.Http;
using IM.Service.Market.Models.Settings;

namespace IM.Service.Market.Settings;

public class ClientSettings
{
    public HostModel Moex { get; set; } = null!;
    public HostModel TdAmeritrade { get; set; } = null!;
    public InvestingModel Investing { get; set; } = null!;
}