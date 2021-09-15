namespace IM.Service.Company.Prices.Settings.Client
{
    public abstract class ClientSettings
    {
        public HostModel Moex { get; set; } = null!;
        public HostModel TdAmeritrade { get; set; } = null!;
    }
}
