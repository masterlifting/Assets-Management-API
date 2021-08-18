namespace IM.Gateways.Web.Companies.Api.Settings.Mq
{
    public class QueueParam
    {
        public string Entity { get; set; } = null!;
        public string[] Actions { get; set; } = null!;
    }
}
