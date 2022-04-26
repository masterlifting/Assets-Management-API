using IM.Service.Common.Net.Models.Http;

namespace IM.Service.Market.Models.Settings;

public class InvestingModel : HostModel
{
    public string Path { get; set; } = null!;
    public string Financial { get; set; } = null!;
    public string Balance { get; set; } = null!;
    public string Dividends { get; set; } = null!;
}