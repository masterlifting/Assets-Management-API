using CommonServices.Models.Http;

namespace IM.Gateway.Companies.Settings.Client
{
    public class ClientSettings
    {
        public HostModel CompanyPrices { get; set; } = null!;
        public HostModel CompanyReports { get; set; } = null!;
        public HostModel CompanyAnalyzer { get; set; } = null!;
    }
}
