using CommonServices.Models.Http;

namespace IM.Service.Company.Reports.Settings.Client
{
    public class InvestingModel : HostModel
    {
        public string Path { get; set; } = null!;
        public string Financial { get; set; } = null!;
        public string Balance { get; set; } = null!;
        public string Dividends { get; set; } = null!;
    }
}
