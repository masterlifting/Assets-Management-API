using IM.Service.Common.Net.Models.Dto.Http;

namespace IM.Service.Company.Data.Settings.Client
{
    public class InvestingModel : HostModel
    {
        public string Path { get; set; } = null!;
        public string Financial { get; set; } = null!;
        public string Balance { get; set; } = null!;
        public string Dividends { get; set; } = null!;
    }
}
