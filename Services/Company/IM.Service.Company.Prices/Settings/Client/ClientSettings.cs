using CommonServices.Models.Http;

namespace IM.Service.Company.Prices.Settings.Client
{
    public class ClientSettings
    {
        public HostModel Moex { get; set; } = null!;
        public HostModel TdAmeritrade { get; set; } = null!;
    }
}
