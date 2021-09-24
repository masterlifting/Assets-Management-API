using CommonServices.Models.Http;

namespace Gateway.Api.Settings.Client
{
    public class ClientSettings
    {
        public HostModel Companies { get; set; } = null!;
        public HostModel CompanyPrices { get; set; } = null!;
        public HostModel CompanyReports { get; set; } = null!;
        public HostModel CompanyAnalyzer { get; set; } = null!;
    }
}
