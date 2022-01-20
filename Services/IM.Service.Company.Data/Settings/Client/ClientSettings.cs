using IM.Service.Common.Net.Models.Dto.Http;

namespace IM.Service.Company.Data.Settings.Client
{
    public class ClientSettings
    {
        public HostModel Moex { get; set; } = null!;
        public HostModel TdAmeritrade { get; set; } = null!;
        public InvestingModel Investing { get; set; } = null!;
    }
}
